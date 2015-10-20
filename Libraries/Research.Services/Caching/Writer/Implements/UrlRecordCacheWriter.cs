using System.Collections.Generic;
using Research.Core.Domain.Seo;
using Research.Core.Events;
using Research.Services.Events;
using Research.Services.Caching.Models;
using Research.Core.Caching;
using System;

namespace Research.Services.Caching.Writer.Implements
{
    public class UrlRecordCacheWriter : BaseCacheWriter, IUrlRecordCacheWriter,
        ICacheConsumer<EntityInserted<UrlRecord>>,
        ICacheConsumer<EntityUpdated<UrlRecord>>,
        ICacheConsumer<EntityDeleted<UrlRecord>>,
        ICacheConsumer<EntityAllChange<UrlRecord>>
    {
        public UrlRecordCachePackage GetAll(Func<UrlRecordCachePackage> acquire)
        {
            return GetFunc(CacheKey.URLRECORD_ALL_KEY, acquire, true, false);
        }

        public UrlRecordForCaching GetBySlug(string slug, Func<UrlRecordForCaching> acquire)
        {
            if (string.IsNullOrEmpty(slug)) return null;
            string key = string.Format(CacheKey.URLRECORD_BY_SLUG_KEY, slug.ToLowerInvariant());
            return GetFunc(key, acquire, true, false);
        }

        public string GetByKeys(int entityId, string entityName, int languageId,
            Func<string> acquire)
        {
            string key = string.Format(CacheKey.URLRECORD_ACTIVE_BY_ID_NAME_LANGUAGE_KEY, entityId, entityName, languageId);
            return GetFunc(key, acquire, true, false);
        }

        public bool TryGetByKeys(int entityId, string entityName, int languageId,
            out string result)
        {
            string key = string.Format(CacheKey.URLRECORD_ACTIVE_BY_ID_NAME_LANGUAGE_KEY, entityId, entityName, languageId);
            return StaticCache.TryGet(key, out result);
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.URLRECORD_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<UrlRecord> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<UrlRecord> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<UrlRecord> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<UrlRecord> eventMessage,
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
