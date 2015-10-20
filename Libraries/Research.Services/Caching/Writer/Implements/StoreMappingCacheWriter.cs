using Research.Core.Domain.Stores;
using Research.Core.Events;
using Research.Services.Caching.Models;
using Research.Services.Events;
using Research.Core.Caching;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class StoreMappingCacheWriter : BaseCacheWriter, IStoreMappingCacheWriter,
        ICacheConsumer<EntityInserted<StoreMapping>>,
        ICacheConsumer<EntityUpdated<StoreMapping>>,
        ICacheConsumer<EntityDeleted<StoreMapping>>,
        ICacheConsumer<EntityAllChange<StoreMapping>>
    {
        public int[] GetStoresIdsWithAccess(int entityId, string entityName, Func<int[]> acquire)
        {
            string key = string.Format(CacheKey.STOREMAPPING_BY_ENTITYID_NAME_KEY, entityId, entityName);
            return GetFunc(key, acquire, true, false);
        }

        public int Order
        {
            get { return 0; }
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.STOREMAPPING_PATTERN_KEY);

            // clear thêm các dữ liệu cache có liên quan đến storeMapping. Lưu ý là vì StoreMapping đã đc cache static khá ổn định,
            // nên ta cần hạn chế cache static những dữ liệu có liên quan đến StoreMapping. Cache per request
            // thì có thể chấp nhận được, nhưng nếu có thể cũng nên clear đầy đủ
            perRequestCachePrefixes.Add(CacheKey.LANGUAGES_ALL_HASHIDDEN_KEY_PATTERN);
            perRequestCachePrefixes.Add(CacheKey.CURRENCIES_ALL_KEY_WITH_HIDDEN_PATTERN);
        }

        public void HandleEvent(EntityInserted<StoreMapping> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<StoreMapping> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<StoreMapping> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<StoreMapping> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
