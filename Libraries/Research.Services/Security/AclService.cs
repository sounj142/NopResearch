using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Security;
using Research.Core.Interface.Service;
using Research.Core;
using Research.Services.Caching.Writer;
using Research.Core.Domain.Catalog;
using Research.Core.Interface.Data;
using Research.Services.Events;

namespace Research.Services.Security
{
    /// <summary>
    /// cache static danh sách các role CustomerRoleId được ánh xạ với entity cụ thể
    /// </summary>
    public partial class AclService : BaseService<AclRecord>, IAclService
    {
        #region Field, ctor, and property

        private readonly IWorkContext _workContext;
        private readonly ISecurityCacheWriter _cacheWriter;
        private readonly CatalogSettings _catalogSettings;

        public AclService(IRepository<AclRecord> repository,
            IEventPublisher eventPublisher, 
            IWorkContext workContext,
            ISecurityCacheWriter cacheWriter,
            CatalogSettings catalogSettings)
            : base(repository, eventPublisher)
        {
            _workContext = workContext;
            _cacheWriter = cacheWriter;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region method
        public IList<AclRecord> GetAclRecords<T>(T entity) where T : BaseEntity, IAclSupported
        {
            if(entity == null) throw new ArgumentNullException("entity");

            string entityName = typeof(T).Name;
            return _repository.Table
                .Where(p => p.EntityId == entity.Id && p.EntityName == entityName)
                .ToList();
        }

        public void InsertAclRecord<T>(T entity, int customerRoleId) where T : BaseEntity, IAclSupported
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (customerRoleId <= 0) throw new ArgumentOutOfRangeException("customerRoleId");

            Insert(new AclRecord
            {
                EntityId = entity.Id,
                EntityName = typeof(T).Name,
                CustomerRoleId = customerRoleId
            });
        }

        public IList<int> GetCustomerRoleIdsWithAccess<T>(T entity) where T : BaseEntity, IAclSupported
        {
            if (entity == null) throw new ArgumentNullException("entity");

            string entityName = typeof(T).Name;
            return _cacheWriter.GetCustomerRoleIdsWithAccess(entity.Id, entityName, () =>
            {
                return _repository.Table
                .Where(p => p.EntityId == entity.Id && p.EntityName == entityName)
                .Select(p => p.CustomerRoleId)
                .ToArray();
            });
        }

        public bool Authorize<T>(T entity) where T : BaseEntity, IAclSupported
        {
            return Authorize(entity, _workContext.CurrentCustomer);
        }

        public bool Authorize<T>(T entity, Customer customer) where T : BaseEntity, IAclSupported
        {
            if (entity == null || customer == null) return false;

            // mặc định là luôn luôn cho phép thao tác. Chỉ khi hệ thống cho phép chức năng Acl và cờ SubjectToAcl bật thì mới hạn chế
            // quyền thao tác

            if (_catalogSettings.IgnoreAcl || !entity.SubjectToAcl) return true;

            var listCustomerId = GetCustomerRoleIdsWithAccess(entity);
            if(listCustomerId.Count == 0) return false;
            foreach (var r1 in customer.CustomerRoles.Where(c => c.Active))
                foreach (int r2 in listCustomerId)
                    if (r2 == r1.Id) return true;
            return false;
        }

        #endregion
    }
}
