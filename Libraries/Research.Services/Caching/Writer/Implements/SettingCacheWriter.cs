using Research.Core.Configuration;
using Research.Core.Domain.Configuration;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Caching.Models;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    /// <summary>
    /// Dùng static cache
    /// </summary>
    public class SettingCacheWriter : BaseCacheWriter, ISettingCacheWriter,
        ICacheConsumer<EntityInserted<Setting>>,
        ICacheConsumer<EntityUpdated<Setting>>,
        ICacheConsumer<EntityDeleted<Setting>>,
        ICacheConsumer<EntityAllChange<Setting>>
    {
        public IDictionary<string, SettingForCache> GetAll(Func<IDictionary<string, SettingForCache>> acquire)
        {
            return this.GetFunc(CacheKey.SETTINGS_ALL_KEY, acquire, true, false);
        }

        public object GetSetting(int storeId, Type type, Func<object> acquire)
        {
            string key = string.Format(CacheKey.SETTINGS_BY_TYPE, storeId, type.AssemblyQualifiedName);
            return this.GetFunc(key, acquire, true, false);
        }

        public bool TryGetSetting(int storeId, Type type, out object result)
        {
            string key = string.Format(CacheKey.SETTINGS_BY_TYPE, storeId, type.AssemblyQualifiedName);
            var cache = StaticCache;
            return cache.TryGet(key, out result);
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.SETTINGS_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<Setting> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<Setting> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<Setting> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<Setting> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public int Order
        {
            get { return 0; }
        }
    }
}
