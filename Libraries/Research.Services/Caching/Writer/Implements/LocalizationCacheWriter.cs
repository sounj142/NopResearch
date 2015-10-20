using Research.Core.Domain.Localization;
using Research.Core.Events;
using Research.Core.Interface.Data;
using Research.Core.Caching;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class LocalizationCacheWriter : BaseCacheWriter, ILocalizationCacheWriter,
        ICacheConsumer<EntityInserted<LocaleStringResource>>,
        ICacheConsumer<EntityUpdated<LocaleStringResource>>,
        ICacheConsumer<EntityDeleted<LocaleStringResource>>,
        ICacheConsumer<EntityAllChange<LocaleStringResource>>
    {
        public IDictionary<string, KeyValuePair<int, string>> GetAllByLaguageId(int languageId,
            Func<IDictionary<string, KeyValuePair<int, string>>> acquire)
        {
            string key = string.Format(CacheKey.LOCALSTRINGRESOURCES_ALL_KEY, languageId);
            return GetFunc(key, acquire, true, false);
        }

        public string GetByResourceKeyAndLanguageId(string resourceKey, int languageId, Func<string> acquire)
        {
            string key = string.Format(CacheKey.LOCALSTRINGRESOURCES_BY_RESOURCENAME_KEY, languageId, resourceKey);
            return GetFunc(key, acquire, true, false);
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.LOCALSTRINGRESOURCES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<LocaleStringResource> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<LocaleStringResource> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<LocaleStringResource> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public int Order
        {
            get { return 0; }
        }

        public void HandleEvent(EntityAllChange<LocaleStringResource> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
