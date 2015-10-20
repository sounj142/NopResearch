using Research.Core.Domain.Customers;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class CustomerAttributeCacheWriter : BaseCacheWriter, ICustomerAttributeCacheWriter,
        ICacheConsumer<EntityInserted<CustomerAttribute>>,
        ICacheConsumer<EntityUpdated<CustomerAttribute>>,
        ICacheConsumer<EntityDeleted<CustomerAttribute>>,
        ICacheConsumer<EntityAllChange<CustomerAttribute>>,
        ICacheConsumer<EntityInserted<CustomerAttributeValue>>,
        ICacheConsumer<EntityUpdated<CustomerAttributeValue>>,
        ICacheConsumer<EntityDeleted<CustomerAttributeValue>>,
        ICacheConsumer<EntityAllChange<CustomerAttributeValue>>
    {
        public IList<CustomerAttribute> GetAllCustomerAttributes(Func<IList<CustomerAttribute>> acquire)
        {
            return GetFunc(CacheKey.CUSTOMERATTRIBUTES_ALL_KEY, acquire, false, true);
        }

        public CustomerAttribute GetCustomerAttributeById(int id, Func<CustomerAttribute> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERATTRIBUTES_BY_ID_KEY, id);
            return GetFunc(key, acquire, false, true);
        }

        public IList<CustomerAttributeValue> GetCustomerAttributeValues(int customerAttributeId,
            Func<IList<CustomerAttributeValue>> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERATTRIBUTEVALUES_ALL_KEY, customerAttributeId);
            return GetFunc(key, acquire, false, true);
        }

        public CustomerAttributeValue GetCustomerAttributeValueById(int id, Func<CustomerAttributeValue> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERATTRIBUTEVALUES_BY_ID_KEY, id);
            return GetFunc(key, acquire, false, true);
        }

        public int Order
        {
            get { return 0; }
        }

        private void AddCacheToClearCustomerAttribute(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            perRequestCachePrefixes.Add(CacheKey.CUSTOMERATTRIBUTES_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.CUSTOMERATTRIBUTEVALUES_PATTERN_KEY);
        }

        private void AddCacheToClearCustomerAttributeValue(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            perRequestCachePrefixes.Add(CacheKey.CUSTOMERATTRIBUTES_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.CUSTOMERATTRIBUTEVALUES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<CustomerAttribute> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerAttribute(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<CustomerAttribute> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerAttribute(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<CustomerAttribute> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerAttribute(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<CustomerAttribute> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerAttribute(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityInserted<CustomerAttributeValue> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerAttributeValue(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<CustomerAttributeValue> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerAttributeValue(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<CustomerAttributeValue> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerAttributeValue(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<CustomerAttributeValue> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerAttributeValue(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
