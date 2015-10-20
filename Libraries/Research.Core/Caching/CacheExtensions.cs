using System;

namespace Research.Core.Caching
{
    /// <summary>
    /// Ghi chú: Cách đặt khóa _syncObject này gây ra 1 vấn đề nghiêm trọng, đó là tất các các cache MemoryCacheManager và PerRequestCacheManager
    /// sẽ phải xếp hàng chờ nhau để giữ khóa này, đây là 1 nghịch lý vì phạm vi cư trú của MemoryCacheManager và PerRequestCacheManager
    /// là khác nhau và ko có vấn đề bất đồng bộ
    /// Hơn nữa, thông thường, tại 1 thòi điểm sẽ có 1 cái MemoryCacheManager và rất nhiều cái PerRequestCacheManager,
    /// nếu phải xếp hàng chờ thì sẽ có vấn đề rất lớn về hiệu suất thực thi
    /// </summary>
    //public static class CacheExtensions
    //{
    //    private static readonly object _syncObject = new object();

    //    public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire)
    //    {
    //        return Get(cacheManager, key, 60, acquire);
    //    }

    //    public static T Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire)
    //    {
    //        lock(_syncObject)
    //        {
    //            if (cacheManager.IsSet(key)) return cacheManager.Get<T>(key);

    //            var result = acquire();
    //            if (cacheTime > 0) cacheManager.Set(key, result, cacheTime);
    //            return result;
    //        }
    //    }
    //}
}
