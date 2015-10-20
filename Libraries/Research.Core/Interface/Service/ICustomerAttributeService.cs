using Research.Core.Domain.Customers;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Vẫn chưa rõ đây là cái gì, dùng để làm gì ?
    /// </summary>
    public partial interface ICustomerAttributeService
    {
        void Delete(CustomerAttribute entity);

        IList<CustomerAttribute> GetAllCustomerAttributes();

        CustomerAttribute GetById(int id);

        void Insert(CustomerAttribute entity);

        void Update(CustomerAttribute entity);




        void Delete(CustomerAttributeValue entity);

        IList<CustomerAttributeValue> GetCustomerAttributeValues(int customerAttributeId);

        CustomerAttributeValue GetCustomerAttributeValueById(int id);

        void Insert(CustomerAttributeValue entity);

        void Update(CustomerAttributeValue entity);
    }
}
