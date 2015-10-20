using Research.Core.Domain.Customers;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class CustomerAndRoleCacheWriter : BaseCacheWriter, ICustomerAndRoleCacheWriter,
        ICacheConsumer<EntityInserted<Customer>>,
        ICacheConsumer<EntityUpdated<Customer>>,
        ICacheConsumer<EntityDeleted<Customer>>,
        ICacheConsumer<EntityAllChange<Customer>>,
        ICacheConsumer<EntityInserted<CustomerRole>>,
        ICacheConsumer<EntityUpdated<CustomerRole>>,
        ICacheConsumer<EntityDeleted<CustomerRole>>,
        ICacheConsumer<EntityAllChange<CustomerRole>>
    {
        #region Cache methods

        public Customer GetById(int id, Func<Customer> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERS_BY_ID, id);
            return GetFunc(key, acquire, false, true);
        }

        public Customer GetByGuid(Guid guid, Func<Customer> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERS_BY_GUID, guid);
            return GetFunc(key, acquire, false, true);
        }

        public Customer GetByEmail(string email, Func<Customer> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERS_BY_EMAIL, email);
            return GetFunc(key, acquire, false, true);
        }

        public Customer GetBySystemName(string systemName, Func<Customer> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERS_BY_SYSTEMNAME, systemName);
            return GetFunc(key, acquire, false, true);
        }

        public Customer GetByUsername(string username, Func<Customer> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERS_BY_USERNAME, username);
            return GetFunc(key, acquire, false, true);
        }



        public CustomerRole GetCustomerRoleById(int id, Func<CustomerRole> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERROLES_BY_ID, id);
            return GetFunc(key, acquire, false, true);
        }

        public CustomerRole GetCustomerRoleBySystemName(string systemName, Func<CustomerRole> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERROLES_BY_SYSTEMNAME_KEY, systemName);
            return GetFunc(key, acquire, false, true);
        }

        public IList<CustomerRole> GetAllCustomerRole(bool showHidden, Func<IList<CustomerRole>> acquire)
        {
            string key = string.Format(CacheKey.CUSTOMERROLES_ALL_WITH_HIDDEN_KEY, showHidden);
            return GetFunc(key, acquire, false, true);
        }

        #endregion


        #region Cache Event Methods

        private void AddCacheToClearCustomer(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            perRequestCachePrefixes.Add(CacheKey.CUSTOMERS_PATTERN_KEY);
        }

        private void AddCacheToClearCustomerRole(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            perRequestCachePrefixes.Add(CacheKey.CUSTOMERROLES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<Customer> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomer(staticCachePrefixes, perRequestCachePrefixes);
        }


        public void HandleEvent(EntityUpdated<Customer> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomer(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<Customer> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomer(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<Customer> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomer(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityInserted<CustomerRole> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerRole(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<CustomerRole> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerRole(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<CustomerRole> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerRole(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<CustomerRole> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearCustomerRole(staticCachePrefixes, perRequestCachePrefixes);
        }

        public int Order
        {
            get { return 0; }
        }

        #endregion
    }
}
