using System;
using System.Collections.Generic;
using Research.Core;
using Research.Core.Infrastructure;

namespace Research.Core.Caching
{
    /// <summary>
    /// Đối tượng ghi cache cơ sở, cho phép ghi cache ở cả 2 mức PerRequest cache ( mặc định ) và static cache ( tùy chọn )
    /// 
    /// Phạm vi sống : Per Request
    /// 
    /// Có thể để cho các lớp loại này thực thi giao diện IConsumer tương ứng, nhưng hiện tại vẫn dùng các lớp ModelCacheEventConsumer
    /// để xử lý tập trung 1 chỗ
    /// </summary>
    public abstract class BaseCacheWriter
    {
        private ICacheManager _staticCache;
        private IPerRequestCacheManager _perRequestCache;

        protected ICacheManager StaticCache
        {
            get
            {
                if (_staticCache == null)
                    _staticCache = EngineContext.Current.Resolve<ICacheManager>();
                return _staticCache;
            }
        }

        protected IPerRequestCacheManager PerRequestCache
        {
            get
            {
                if (_perRequestCache == null)
                    _perRequestCache = EngineContext.Current.Resolve<IPerRequestCacheManager>();
                return _perRequestCache;
            }
        }

        /// <summary>
        /// Lấy ra T, nguồn từ static hoặc per request cache, tùy chỉ định
        /// </summary>
        protected T GetFunc<T>(string key, Func<T> acquire, bool useStaticCache = false, bool userPerRequestCache = false)
        {
            if (useStaticCache)
            {
                if (userPerRequestCache)
                {
                    var perRequestCache = this.PerRequestCache;
                    return StaticCache.Get(key, () => perRequestCache.Get(key, acquire));
                }
                else return StaticCache.Get(key, acquire);
            }
            else
            {
                if (userPerRequestCache) return PerRequestCache.Get(key, acquire);
                else return acquire();
            }
        }

        /// <summary>
        /// Cưỡng ép ghi đè lên static/per request cache giá trị mới
        /// </summary>
        protected T GetForce<T>(string key, T newValue, bool useStaticCache = false, bool usePerRequestCache = false)
        {
            if (usePerRequestCache) PerRequestCache.Set(key, newValue);
            if (useStaticCache) StaticCache.Set(key, newValue);
            return newValue;
        }
    }
}
