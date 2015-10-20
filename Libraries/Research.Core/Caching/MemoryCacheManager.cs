using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace Research.Core.Caching
{
    public partial class MemoryCacheManager: ICacheManager
    {
        #region thread safe

        private static readonly object _syncObject = new object();

        public virtual T Get<T>(string key, Func<T> acquire)
        {
            return Get(key, 60, acquire);
        }

        //public virtual T Get<T>(string key, int cacheTime, Func<T> acquire)
        //{
        //    if (IsSet(key)) return Get<T>(key);
        //    if (cacheTime <= 0) return acquire(); // ko cache

        //    lock (_syncObject)
        //    {
        //        if (IsSet(key)) return Get<T>(key);

        //        var result = acquire();
        //        Set(key, result, cacheTime);
        //        return result;
        //    }
        //}

        //public virtual T Get<T>(string key, int cacheTime, Func<T> acquire)
        //{
        //    if (IsSet(key)) return Get<T>(key);
        //    if (cacheTime <= 0) return acquire(); // ko cache

        //    // vẫn cho phép thoải mái thực hiện hàm acquire ( hàm với chi phí cao và có thể mất thời gian ) nhiều lần
        //    // nhưng sẽ chỉ có 1 kết quả có cơ hội đi vào cache, và kết quả đó sẽ được sử dụng để làm giá trị trả về. Tức là giả sử
        //    // thao tác Get<T>() có 3 hàm acquire được thực hiện đồng thời, cho ra kết quả A, B, C, nhưng chỉ có B là đi vào cache vậy thì
        //    // kết quả trả về của cả 3 Get<T>() sẽ là B
        //    // => acquire có thể bị thực hiện nhiều lần trước khi được cache. Cần chú ý điều này
            
        //    // => Do đó, khi viết hàm acquire cần đảm bảo là hàm này đủ bền vững để chấp nhận chạy nhiều phiên bản của cùng 1 hàm song song, và đủ độc lập
        //    // để có thể lấy kết quả của 1 trong các hàm chạy song song đó để làm kết quả chung cho tất cả các hàm

        //    // phương pháp lock trực tiếp bao lấy acquire() sẽ gây ra 1 nút nghẽn cổ chai ở đây, đồng thời acquire phải được thiết kế
        //    // để chạy nhanh nhất có thể. 1 chức năng mà tiêu tốn nhiều thời gian, chẳng hạn như kết nối service ở xa để lấy bảng tỷ giá
        //    // thì sẽ ko đc phép đặt vào trong acquire() mà phải có 1 phương pháp phù hợp
        //    var result = acquire(); 
        //    lock (_syncObject)
        //    {
        //        if (IsSet(key)) return Get<T>(key);
        //        Set(key, result, cacheTime);
        //        return result;
        //    }
        //}

        /// <summary>
        /// Acquire cần đảm bảo là thực hiện nhanh, nếu hàm trì hoàn nhiều thì cần tính đến giải pháp khác
        /// </summary>
        public virtual T Get<T>(string key, int cacheTime, Func<T> acquire)
        {
            if (IsSet(key)) return Get<T>(key);
            if (cacheTime <= 0) return acquire(); // ko cache

            // tóm lại là vẫn nên cho acquire() vào log. Điều này tránh được lỗi dữ liệu cache sai được đề cập ở RemoveByPattern(),
            // đồng thời nếu chúng ta khóa trực tiếp ở đây thì cũng giúp cho thao tác remove, clear được thực hiện liên tục, ko bị chen
            // ngang nên sẽ kết thúc nhanh
            // Acquire cần đảm bảo là thực hiện nhanh, nếu hàm trì hoàn nhiều thì cần tính đến giải pháp khác
            // Tình trạng nghẽn cổ chai là có, nhưng ko lâu vì ta chỉ cache khá ít dữ liệu, và sau khi cache đầy đủ rồi thì hệ thống
            // sẽ đi vào ổn định và ko cần lo nghẽn nữa
            lock (_syncObject)
            {
                if (IsSet(key)) return Get<T>(key);
                var result = acquire();
                SetNotRequiredLock(key, result, cacheTime);
                return result;
            }
        }

        #endregion
        

        protected virtual ObjectCache Cache
        {
            get { return MemoryCache.Default; } // ko dùng DI ?
        }

        public virtual T Get<T>(string key)
        {
            return (T)Cache[key];
        }

        public virtual bool TryGet<T>(string key, out T value)
        {
            if(Cache.Contains(key))
            {
                value = (T)Cache[key];
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public virtual void Set<T>(string key, T data, int cacheTime)
        {
            lock(_syncObject)
            {
                SetNotRequiredLock(key, data, cacheTime);
            }
        }

        private void SetNotRequiredLock<T>(string key, T data, int cacheTime)
        {
            // lý do ko cho phép add null : Là do có những lúc khóa dùng để cache là đến từ nguồn ko đáng tin tưởng, vd như khi ta cache
            // theo chuỗi slug dùng cho chức năng Url 1 phân đoạn, chúng ta sẽ đi từ chuỗi slug do người dùng nhập trong url => đọc
            // database để tìm UrlRecord tương ứng => Lưu cache nếu cần thiết. Ở đây, nếu ngay cả null cũng lưu cache vậy thì chỉ cần
            // người dùng cung cấp random 1 tỷ cái url đểu thì chúng ta sẽ cache 1 tỷ cái null, nát bộ nhớ ngay
            // => chỉ cache khi tìm thấy khác null, trường hợp null ( dù là dữ liệu đúng và có thể cache ) thì chấp nhận đọc lại database
            if (data == null) return;

            Cache.Set(new CacheItem(key, data),
                new CacheItemPolicy { AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime) });
        }

        public virtual bool IsSet(string key)
        {
            return Cache.Contains(key);
        }

        public virtual void Remove(string key)
        {
            lock (_syncObject)
            {
                Cache.Remove(key);
            }
        }

        //public virtual void RemoveByPattern(string pattern)
        //{
        //    var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //    var keyToRemove = new List<string>();
            
        //    var cache = Cache;
        //    foreach(var item in cache)
        //        if (regex.IsMatch(item.Key))
        //            keyToRemove.Add(item.Key);
            
        //    // khóa lại trước khi clear hoặc remove để ngăn chặn 1 luồng khác kiểm tra thấy 1 key bị thiếu và tự ghi đè giá trị chen ngang
        //    // quá trình clear. Vấn đề càng nghiêm trọng hơn trong trường hợp giá trị cache phụ thuộc nhau
        //    // VD: Có 2 giá trị cache A, B, trong đó B được tính từ giá trị của A. Thao tác clear cache muốn clear cả A và B
        //    // nhưng chỉ mới clear B, thì 1 luồng khác chen ngang, luồng này "muốn B", và kiểm tra thấy ko có B nên nó lấy A ( cũ chưa
        //    // clear ) ra và tính ra B mới ( B này cũng dùng dữ liệu cũ nên sai ). Đến thời điểm này, tùy trường hợp có lock ở hàm
        //    // get(acquire) hay ko, nếu lock thì luồng kia chờ, còn ko thì nó ghi B vào trực tiếp. Nhưng nhìn chung thì sau khi clear
        //    // thì A bị xóa, nhưng B mới đc lưu vào sẽ mang giá trị sai

        //    // => nếu acquire() bị đặt trong lock sẽ ko có vấn đề
        //    lock (_syncObject) 
        //    {
        //        foreach (var key in keyToRemove) cache.Remove(key); ////NULL nguyên bản gọi this.Remove()
        //    }
        //}

        public virtual void RemoveByPattern(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keyToRemove = new List<string>();

            var cache = Cache;
            
            // khóa lại trước khi clear hoặc remove để ngăn chặn 1 luồng khác kiểm tra thấy 1 key bị thiếu và tự ghi đè giá trị chen ngang
            // quá trình clear. Vấn đề càng nghiêm trọng hơn trong trường hợp giá trị cache phụ thuộc nhau
            // VD: Có 2 giá trị cache A, B, trong đó B được tính từ giá trị của A. Thao tác clear cache muốn clear cả A và B
            // nhưng chỉ mới clear B, thì 1 luồng khác chen ngang, luồng này "muốn B", và kiểm tra thấy ko có B nên nó lấy A ( cũ chưa
            // clear ) ra và tính ra B mới ( B này dùng dữ liệu A cũ nên sai ). Đến thời điểm này, tùy trường hợp có lock ở hàm
            // get(acquire) hay ko, nếu lock thì luồng kia chờ, còn ko thì nó ghi B vào trực tiếp. Nhưng nhìn chung thì sau khi clear
            // thì A bị xóa, nhưng B mới đc lưu vào sẽ mang giá trị sai

            // => nếu acquire() bị đặt trong lock sẽ ko có vấn đề
            lock (_syncObject)
            {
                // thử đẩy khối code foreach vào trong khóa để ngăn ko cho tập cache bị thay đổi giữa 2 thao tác foreach tìm khóa
                // và clear danh sách khoá
                foreach (var item in cache)
                    if (regex.IsMatch(item.Key))
                        keyToRemove.Add(item.Key);

                foreach (var key in keyToRemove) cache.Remove(key); ////NULL nguyên bản gọi this.Remove()
            }
        }

        //public virtual void Clear()
        //{
        //    var cache = Cache;
        //    var listKey = cache.Select(p => p.Key).ToList();
        //    lock (_syncObject)
        //    {
        //        foreach (var key in listKey) cache.Remove(key); ////NULL nguyên bản gọi this.Remove()
        //    }
        //}

        public virtual void Clear()
        {
            var cache = Cache;
            
            lock (_syncObject)
            {
                var listKey = cache.Select(p => p.Key).ToList();
                foreach (var key in listKey) cache.Remove(key); ////NULL nguyên bản gọi this.Remove()
            }
        }


        public void Set<T>(string key, T data)
        {
            Set(key, data, 60);
        }


        public void RemoveByKeysStartsWith(string prefix)
        {
            if (prefix == null) return;
            var keyToRemove = new List<string>();

            var cache = Cache;
            
            lock (_syncObject)
            {
                foreach (var item in cache)
                    if (item.Key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                        keyToRemove.Add(item.Key);
                foreach (var key in keyToRemove) cache.Remove(key); ////NULL nguyên bản gọi this.Remove()
            }
        }

        public void RemoveByKeysStartsWith(IList<string> prefixes)
        {
            if (prefixes == null || prefixes.Count == 0) return;
            var keyToRemove = new List<string>();

            var cache = Cache;

            // Ở trong lần chỉnh sửa đầu tiên ( code nguyên bản Nop ), khi mà code lấy danh sách khóa thỏa mãn mẫu và code remove tách rời
            // và đều ko có lock thì việc remove ko hết, hay trong lúc remove thì có 1 khóa đã bị xóa trước đó lại sống lại là có thể xảy ra

            // Ở code chỉnh sửa lần 2, khi mà đoạn code remove đã được lock, nhưng code tìm khóa cần remove vẫn đặt ngoài lock khi đó 
            // tình huống đang remove thì 1 khóa đã remove sống lại ko xảy ra ( vì code Get(Func<> acquire) được đặt trong lock )
            // nhưng do khoảng thời gian giữa thao tác lấy danh sách khóa và thao tác lock để clear, vẫn có khả năng remove thiếu

            // Ở lần 3, cả code tìm key phù hợp và code remove được đặt vào lock, khi đó tình huống remove thiếu ko xảy ra, nhưng
            // vẫn sẽ phát sinh vấn đề với các thao tác Remove theo các mẫu key khác nhau
            // VD :
            // StaticCache.RemoveByKeysStartsWith(KEY1);
            // StaticCache.RemoveByKeysStartsWith(KEY2);
            // 2 thao tác này đc đặt cạnh nhau nhưng hoàn toàn có thể bị thực thi rời rạc và bị các thao tác ghi cache xen ngang. Sự 
            // xen ngang này có thể phát sinh vấn đề nếu như cache này phụ thuộc vào cache kia. Chẳng hạn cache KEY1 được lấy từ dữ liệu KEY2
            // Khi KEY1 đc clear, thao tác clear KEY2 bị chen ngang bởi 1 thao tác get KEY1, và thao tác get này lại đọc KEY1 từ dữ
            // liệu KEY2 vốn đang ra bị clear

            // Ở lần 4, ta đã thử nghiệm đổi IConsumer thành ICacheConsumer để chỉ phục vụ thao tác clear cache. Để làm điều này, các
            // ICacheConsumer sẽ ko tự mình remove cache mà nhận sự kiện, sau đó tự mình xử lý và quyết định xem sẽ remove cache nào
            //, những mẫu cache được remove sẽ được add vào 1 List ( có 1 list cho static, 1 list cho cache PerRequest )
            // Kết thúc lặp qua tất cả các ICacheConsumer. EventPublish sẽ yêu cầu 1 thao tác Remove duy nhất trên tập các mẫu key
            // mà nó tổng hợp được. Thao tác này là tập trung, có lock, nên sẽ đảm bảo việc remove cache là sạch sẽ và ko để đám
            // zombie dữ liệu cũ sống dậy

            // Nếu muốn 1 IConsumer thuần túy xử lý sự kiện, có thể viết lại 1 IConsumer như của Nop, có điều vòng lặp thực thi các IConsumer
            // cần được để sau vòng lặp thực thi các ICacheConsumer và thao tác remove cache.
            // Thực ra cơ chế event IConsumer trong NOP hoàn toàn là để clear cache, cho nên khi chuyển nó thành ICacheConsumer cũng chả có
            // vấn đề gì

            lock (_syncObject)
            {
                foreach (var item in cache)
                {
                    foreach (var prefix in prefixes)
                        if (item.Key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            keyToRemove.Add(item.Key);
                            break;
                        }
                }

                foreach (var key in keyToRemove) cache.Remove(key); ////NULL nguyên bản gọi this.Remove()
            }
        }
    }
}
