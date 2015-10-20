using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Security;
using Research.Core.Interface.Service;
using Research.Core;
using Research.Services.Caching.Writer;
using Research.Core.Interface.Data;
using Research.Services.Events;
using Research.Core.Infrastructure;
using Research.Core.Events;
using Research.Core.Domain.Localization;

namespace Research.Services.Security
{
    public partial class PermissionService : BaseService<PermissionRecord>, IPermissionService
    {
        #region field, ctor, property

        private readonly IWorkContext _workContext;
        private readonly ISecurityCacheWriter _cacheWriter;

        public PermissionService(IRepository<PermissionRecord> repository,
            IEventPublisher eventPublisher,
            IWorkContext workContext,
            ILanguageService languageService,
            ISecurityCacheWriter cacheWriter)
            : base(repository, eventPublisher)
        {
            _workContext = workContext;
            _cacheWriter = cacheWriter;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Kiểm tra xem role customerRole có quyền con permissionRecordSystemName hay ko ( kết quả đc cache static ).
        /// Lưu ý là customerRole phải lấy trực tiếp từ database
        /// </summary>
        protected virtual bool Authorize(string permissionRecordSystemName, CustomerRole customerRole)
        {
            if (string.IsNullOrEmpty(permissionRecordSystemName) || customerRole == null) return false;

            return _cacheWriter.PermissionRecordMapCustomerRole(permissionRecordSystemName, customerRole.Id, () => {
                // thao tác lấy về tập hợp có liên kết thông qua lazy loading ( dạng customerRole.PermissionRecords ) sẽ luôn luôn
                // lấy về tất cả dữ liệu có liên quan ( select all ), cho nên thao tác phía sau nó ( .Any() , .FirstOrDefault() ...
                // sẽ ko ảnh hưởng đến cấu truy vấn select all tạo thành, mà các thao tác sau này sẽ được chạy trên kết quả của
                // câu truy vấn đc đặt trong bộ nhớ, nên khi so sánh chuỗi, nó sẽ so sánh có phân biệt hoa thường chứ ko như so sánh 
                // trong database
                return customerRole.PermissionRecords.Any(p => 
                    permissionRecordSystemName.Equals(p.SystemName, StringComparison.InvariantCultureIgnoreCase));
            });
        }

        #endregion

        #region Methods

        public PermissionRecord GetPermissionRecordBySystemName(string systemName)
        {
            if (string.IsNullOrEmpty(systemName)) return null;

            return _repository.Table.FirstOrDefault(p => p.SystemName == systemName);
        }

        public IList<PermissionRecord> GetAllPermissionRecords()
        {
            return _repository.Table.OrderBy(p => p.Name).ToList();
        }

        public void InstallPermissions(IPermissionProvider permissionProvider)
        {
            if (permissionProvider == null) throw new ArgumentNullException("permissionProvider");

            var engine = EngineContext.Current;
            var localizationService = engine.Resolve<ILocalizationService>();
            var languageService = engine.Resolve<ILanguageService>();

            using (var transaction = _unitOfWork.BeginTransaction())
            {
                var permissions = permissionProvider.GetPermissions();

                // insert PermissionRecord trước
                foreach (var p in permissions)
                {
                    var permission = GetPermissionRecordBySystemName(p.SystemName);
                    if (permission == null)
                    {
                        var permissionRecord = new PermissionRecord
                        {
                            Category = p.Category,
                            Name = p.Name,
                            SystemName = p.SystemName
                        };
                        _repository.Insert(permissionRecord);
                        // lưu chuỗi dịch cho tất cả các PermissionRecord vào bảng LocaleStringResource
                        permissionRecord.SaveLocalizedPermissionName(localizationService, languageService, false);
                        _unitOfWork.SaveChanges();
                    }
                }

                // insert các CustomerRole
                var defaultPermissions = permissionProvider.GetDefaultPermissions();
                var customerRoleRepo = engine.Resolve<IRepository<CustomerRole>>();
                var customerRoleService = engine.Resolve<ICustomerService>();
                foreach (var c in defaultPermissions)
                {
                    var customerRole = customerRoleService.GetCustomerRoleBySystemName(c.CustomerRoleSystemName);
                    if (customerRole == null)
                    {
                        customerRoleRepo.Insert(new CustomerRole
                        {
                            Name = c.CustomerRoleSystemName,
                            Active = true,
                            SystemName = c.CustomerRoleSystemName
                        });
                        _unitOfWork.SaveChanges();
                    }
                }


                var allPermision = _repository.Table.ToList();
                // ghi nhận các ánh xạ CustomerRole - PermissionRecord
                foreach (var c in defaultPermissions)
                {
                    var customerRole = customerRoleService.GetCustomerRoleBySystemName(c.CustomerRoleSystemName);
                    if (customerRole == null) continue;

                    var addList = new List<PermissionRecord>();
                    foreach (var permission in c.PermissionRecords)
                        if (customerRole.PermissionRecords.All(q => !string.Equals(permission.SystemName, q.SystemName,
                            StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var p1 = allPermision.FirstOrDefault(q => string.Equals(permission.SystemName, q.SystemName,
                                StringComparison.InvariantCultureIgnoreCase));
                            if (p1 != null) addList.Add(p1);
                        }
                    if (addList.Count > 0)
                    {
                        foreach (var permision in addList)
                            customerRole.PermissionRecords.Add(permision);
                        _unitOfWork.SaveChanges();
                    }
                }

                transaction.Commit();

                var event1 = new EntityAllChange<PermissionRecord>((PermissionRecord)null);
                var event2 = new EntityAllChange<CustomerRole>((CustomerRole)null);
                var event3 = new EntityAllChange<LocaleStringResource>((LocaleStringResource)null);
                _eventPublisher.Publish(event1, event2, event3);
            }
        }

        public void UninstallPermissions(IPermissionProvider permissionProvider)
        {
            if (permissionProvider == null) throw new ArgumentNullException("permissionProvider");

            var engine = EngineContext.Current;
            var localizationService = engine.Resolve<ILocalizationService>();
            var languageService = engine.Resolve<ILanguageService>();

            foreach(var p in permissionProvider.GetPermissions())
            {
                var permission = GetPermissionRecordBySystemName(p.SystemName);
                if (permission != null) 
                {
                    _repository.Delete(permission);
                    permission.DeleteLocalizedPermissionName(localizationService, languageService, false);
                }
            }
            _unitOfWork.SaveChanges();

            var event1 = new EntityAllChange<PermissionRecord>((PermissionRecord)null);
            var event2 = new EntityAllChange<LocaleStringResource>((LocaleStringResource)null);
            _eventPublisher.Publish(event1, event2);
        }

        public bool Authorize(PermissionRecord permission)
        {
            return Authorize(permission, _workContext.CurrentCustomer);
        }

        public bool Authorize(PermissionRecord permission, Customer customer)
        {
            if (permission == null) return false;
            return Authorize(permission.SystemName, customer);
        }

        public bool Authorize(string permissionRecordSystemName)
        {
            return Authorize(permissionRecordSystemName, _workContext.CurrentCustomer);
        }

        public bool Authorize(string permissionRecordSystemName, Customer customer)
        {
            if (string.IsNullOrEmpty(permissionRecordSystemName) || customer == null) return false;

            // vẫn có vẻ ko hiệu quả, khi với mỗi request cần kiểm tra quyền con, cần ít nhất là 2 truy vấn :
            // + Truy vấn lấy về customer theo email/username
            // + Truy vấn lấy về tất cả các CustomerRoles của customer ấy
            // Tuy nhiên, việc cache customer hoặc các customerRole của customer đều gây ra những khó khăn về mặt kỹ thuật, nhất là việc
            // đảm bảo clear cache đúng, đầy đủ, đúng lúc và ko sai sót => cứ dùng cơ chế như hiện nay
            foreach (var role in customer.CustomerRoles.Where(p => p.Active))
                if (Authorize(permissionRecordSystemName, role)) return true;

            return false;
        }

        #endregion
    }
}
