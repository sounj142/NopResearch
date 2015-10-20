using Research.Core.Domain.Customers;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface ICustomerAttributeCacheWriter
    {
        /// <summary>
        /// lấy về tất cả các CustomerAttribute trong database, per request cache
        /// </summary>
        IList<CustomerAttribute> GetAllCustomerAttributes(Func<IList<CustomerAttribute>> acquire);

        /// <summary>
        /// lấy về CustomerAttribute theo id, per request cache
        /// </summary>
        CustomerAttribute GetCustomerAttributeById(int id, Func<CustomerAttribute> acquire);

        /// <summary>
        /// Gets customer attribute values by customer attribute identifier, cache per request
        /// </summary>
        IList<CustomerAttributeValue> GetCustomerAttributeValues(int customerAttributeId,
            Func<IList<CustomerAttributeValue>> acquire);

        /// <summary>
        /// Get CustomerAttributeValue theo id, per request cache
        /// </summary>
        CustomerAttributeValue GetCustomerAttributeValueById(int id, Func<CustomerAttributeValue> acquire);
    }
}
