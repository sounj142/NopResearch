using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Core.Domain.Stores
{
    public static class StoreExtensions
    {
        /// <summary>
        /// Parse cột Hosts của đối tượng store ( chuỗi các host cách nhau bởi dấu phẩy ) thành 1 mảng các host riêng lẻ.
        /// Một giá trị mẫu : "localhost,vietthang.localhost.com"
        /// </summary>
        public static IList<string> ParseHostValues(this Store store)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(store.Hosts)) return result;

            foreach (string host in store.Hosts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var tmp = host.Trim();
                if (!string.IsNullOrEmpty(tmp)) result.Add(tmp);
            }
            return result;
        }

        /// <summary>
        /// Kiểm tra xem trong danh sách host của property Store.Hosts có chứa host ko. Hàm này sẽ thực hiện split chuỗi Store.Hosts
        /// rồi mới so sánh cho nên chỉ nên gọi khi chỉ có 1 chuỗi host duy nhất cần so sánh. Nếu có nhiều chuỗi host cần kiểm tra,
        /// cần dùng hàm ParseHostValues() rồi sau đó tự foreah và kiểm tra, như thế sẽ hiệu quả hơn
        /// </summary>
        public static bool ContainsHostValue(this Store store, string host)
        {
            if (string.IsNullOrEmpty(host)) return false;
            return store.ParseHostValues().Contains(host, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
