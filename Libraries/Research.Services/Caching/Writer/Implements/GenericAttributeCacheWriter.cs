using Research.Core.Domain.Common;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class GenericAttributeCacheWriter : BaseCacheWriter, IGenericAttributeCacheWriter,
        ICacheConsumer<EntityInserted<GenericAttribute>>,
        ICacheConsumer<EntityUpdated<GenericAttribute>>,
        ICacheConsumer<EntityDeleted<GenericAttribute>>,
        ICacheConsumer<EntityAllChange<GenericAttribute>>
    {
        public IList<GenericAttribute> GetAttributesForEntity(int entityId, string keyGroup,
            Func<IList<GenericAttribute>> acquire)
        {
            string key = string.Format(CacheKey.GENERICATTRIBUTE_KEY, entityId, keyGroup);
            return GetFunc(key, acquire, false, true);
        }

        public TPropType GetAttributesForEntity<TPropType>(int entityId, string keyGroup, 
            string key, int storeId, Func<TPropType> acquire)
        {
            string keyCache = string.Format(CacheKey.GENERICATTRIBUTE_BYENTITY_KEY, entityId, keyGroup, key, storeId);
            return GetFunc(keyCache, acquire, false, true);
        }

        /// <summary>
        /// Nhận xét: Khi sử dụng cách thức add cache vào 1 list để clear 1 lần duy nhất thì thứ tự Order đã ko còn quan trọng nữa
        /// </summary>
        public int Order
        {
            get { return 0; }
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            perRequestCachePrefixes.Add(CacheKey.GENERICATTRIBUTE_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<GenericAttribute> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<GenericAttribute> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<GenericAttribute> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<GenericAttribute> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
