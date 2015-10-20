using Research.Core.Domain.Customers;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Services.Customers
{
    // service hiện sẽ dùng perrequest cache như Nop
    public partial class CustomerAttributeService : BaseService<CustomerAttribute>, ICustomerAttributeService
    {
        #region Fields, ctors, properties

        private readonly IRepository<CustomerAttributeValue> _customerAttributeValueRepository;
        private readonly ICustomerAttributeCacheWriter _cacheWriter;

        public CustomerAttributeService(IRepository<CustomerAttribute> repository, 
            IEventPublisher eventPublisher,
            IRepository<CustomerAttributeValue> customerAttributeValueRepository,
            ICustomerAttributeCacheWriter cacheWriter)
            :base (repository, eventPublisher)
        {
            _customerAttributeValueRepository = customerAttributeValueRepository;
            _cacheWriter = cacheWriter;
        }

        #endregion

        #region Methods

        public IList<CustomerAttribute> GetAllCustomerAttributes()
        {
            return _cacheWriter.GetAllCustomerAttributes(() => _repository.Table.OrderBy(c => c.DisplayOrder).ToList());
        }

        public override CustomerAttribute GetById(int id)
        {
            if (id <= 0) return null;
            return _cacheWriter.GetCustomerAttributeById(id, () => _repository.GetById(id));
        }

        public void Delete(CustomerAttributeValue entity)
        {
            _customerAttributeValueRepository.Delete(entity);
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityDeleted(entity);
        }

        public IList<CustomerAttributeValue> GetCustomerAttributeValues(int customerAttributeId)
        {
            return _cacheWriter.GetCustomerAttributeValues(customerAttributeId, () =>
            {
                return _customerAttributeValueRepository.Table
                    .Where(p => p.CustomerAttributeId == customerAttributeId)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList();
            });
        }

        public CustomerAttributeValue GetCustomerAttributeValueById(int id)
        {
            if (id <= 0) return null;
            return _cacheWriter.GetCustomerAttributeValueById(id, () => _customerAttributeValueRepository.GetById(id));
        }

        public void Insert(CustomerAttributeValue entity)
        {
            _customerAttributeValueRepository.Insert(entity);
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(CustomerAttributeValue entity)
        {
            _customerAttributeValueRepository.Update(entity);
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityUpdated(entity);
        }

        #endregion
    }
}
