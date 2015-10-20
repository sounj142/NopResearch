using Research.Core.Domain.Stores;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Caching.Models;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class StoreCacheWriter : BaseCacheWriter, IStoreCacheWriter,
        ICacheConsumer<EntityInserted<Store>>,
        ICacheConsumer<EntityUpdated<Store>>,
        ICacheConsumer<EntityDeleted<Store>>,
        ICacheConsumer<EntityAllChange<Store>>
    {
        public IList<StoreForCache> GetAll(Func<IList<StoreForCache>> acquire)
        {
            return GetFunc(CacheKey.STORES_ALL_KEY, acquire, true, false);
        }

        public Store GetById(int storeId, Func<Store> acquire)
        {
            string key = string.Format(CacheKey.STORES_BY_ID_KEY, storeId);
            return GetFunc(key, acquire, false, true);
        }

        public int Order
        {
            get { return 0; }
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.STORES_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.STORES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<Store> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<Store> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<Store> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<Store> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
