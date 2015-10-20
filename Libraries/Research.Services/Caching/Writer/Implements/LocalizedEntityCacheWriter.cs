using Research.Core.Domain.Localization;
using Research.Core.Events;
using Research.Core.Interface.Data;
using Research.Core.Caching;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class LocalizedEntityCacheWriter: BaseCacheWriter, ILocalizedEntityCacheWriter,
        ICacheConsumer<EntityInserted<LocalizedProperty>>,
        ICacheConsumer<EntityDeleted<LocalizedProperty>>,
        ICacheConsumer<EntityUpdated<LocalizedProperty>>,
        ICacheConsumer<EntityAllChange<LocalizedProperty>>
    {
        public IDictionary<string, string> GetAll(Func<IDictionary<string, string>> acquire)
        {
            return GetFunc(CacheKey.LOCALIZEDPROPERTY_ALL_KEY, acquire, true, false);
        }

        public string Get(int languageId, int entityId, string localeKeyGroup, string localeKey, Func<string> acquire)
        {
            string key = string.Format(CacheKey.LOCALIZEDPROPERTY_KEY, languageId, entityId, localeKeyGroup, localeKey);
            return GetFunc(key, acquire, true, false);
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.LOCALIZEDPROPERTY_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<LocalizedProperty> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<LocalizedProperty> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<LocalizedProperty> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<LocalizedProperty> eventMessage,
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
