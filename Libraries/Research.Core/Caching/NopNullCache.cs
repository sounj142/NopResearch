using System.Collections.Generic;

namespace Research.Core.Caching
{
    public partial class NopNullCache : ICacheManager, IPerRequestCacheManager
    {
        public virtual T Get<T>(string key)
        {
            return default(T); // ko chính xác nếu dùng với các kiểu nguyên thủy
        }

        public virtual bool TryGet<T>(string key, out T value)
        {
            value = default(T);
            return false;
        }

        public virtual void Set<T>(string key, T data, int cacheTime)
        {
        }

        public virtual bool IsSet(string key)
        {
            return false;
        }

        public virtual void Remove(string key)
        {
        }

        public virtual void RemoveByPattern(string pattern)
        {
        }

        public virtual void Clear()
        {
        }


        public virtual T Get<T>(string key, System.Func<T> acquire)
        {
            return Get(key, 60, acquire);
        }

        public virtual T Get<T>(string key, int cacheTime, System.Func<T> acquire)
        {
            return acquire();
        }


        public void Set<T>(string key, T data)
        {
            Set(key, data, 60);
        }


        public void RemoveByKeysStartsWith(string prefix)
        {
        }

        public void RemoveByKeysStartsWith(IList<string> prefixes)
        {
        }
    }
}
