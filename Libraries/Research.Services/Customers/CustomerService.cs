using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Customers;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Core;
using Research.Core.Domain.Common;
using Research.Services.Events;
using Research.Services.Caching.Writer;
using Research.Core.Domain.Orders;
using System.Globalization;
using Research.Core.Domain.Shipping;
using Research.Core.Events;

namespace Research.Services.Customers
{
    /// <summary>
    /// CustomerService sẽ xử lý chung các entity Customer và CustomerRole, bao gồm cả các Repository của chúng
    /// 
    /// Sẽ cache static tất cả các CustomerRole, và chỉ cache per request các Customer. Cần chú ý clear đầy đủ nếu phát sinh tình huống 
    /// dữ liệu cache phụ thuộc nhau nhiều cấp ???
    /// </summary>
    public class CustomerService : BaseService<Customer>, ICustomerService
    {
        #region Fields, Properties, and Ctors

        private readonly ICustomerRepository _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IGenericAttributeService _genericAttributeService; // vừa dùng repository vừa dùng service của GenericAttribute
        private readonly ICustomerAndRoleCacheWriter _cacheWriter;
        private readonly CustomerSettings _customerSettings;
        private readonly CommonSettings _commonSettings;

        public CustomerService(ICustomerRepository repository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            IGenericAttributeService genericAttributeService,
            ICustomerAndRoleCacheWriter cacheWriter,
            IEventPublisher eventPublisher,
            CustomerSettings customerSettings,
            CommonSettings commonSettings)
            : base(repository, eventPublisher)
        {
            _customerRepository = repository;
            _customerRoleRepository = customerRoleRepository;
            _genericAttributeRepository = genericAttributeRepository;
            _genericAttributeService = genericAttributeService;
            _cacheWriter = cacheWriter;
            _customerSettings = customerSettings;
            _commonSettings = commonSettings;
        }

        #endregion


        #region Methods


        #region Customers

        public override Customer GetById(int id)
        {
            if (id <= 0) return null;
            return _cacheWriter.GetById(id, () => _repository.GetById(id));
        }

        public virtual IPagedList<Customer> GetAllCustomers(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, 
            int affiliateId = 0, int vendorId = 0, int[] customerRoleIds = null, string email = null, string username = null, 
            string firstName = null, string lastName = null, int dayOfBirth = 0, int monthOfBirth = 0, string company = null, 
            string phone = null, string zipPostalCode = null, bool loadOnlyWithShoppingCart = false, ShoppingCartType? sct = null, 
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _repository.Table.Where(c => !c.Deleted);
            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => c.CreatedOnUtc <= createdToUtc.Value);
            if(affiliateId > 0)
                query = query.Where(c => c.AffiliateId == affiliateId);
            if (vendorId > 0)
                query = query.Where(c => c.VendorId == vendorId);
            if (customerRoleIds != null && customerRoleIds.Length > 0)
                query = query.Where(c => c.CustomerRoles.Select(r => r.Id).Intersect(customerRoleIds).Any());
            if(!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));

            // bởi vì firstName không nằm trong bảng Customer mà nó nằm trong bảng generic attribute nên ta sẽ phải thực hiện 1 phép join
            // đến bảng này để tiến hành lọc dữ liệu
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Join(_genericAttributeRepository.Table, x => x.Id, y => y.EntityId,
                        (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == "Customer"
                        && z.Attribute.Key == SystemCustomerAttributeNames.FirstName
                        && z.Attribute.Value.Contains(firstName))
                    .Select(z => z.Customer);
            }
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Join(_genericAttributeRepository.Table, x => x.Id, y => y.EntityId,
                        (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == "Customer"
                        && z.Attribute.Key == SystemCustomerAttributeNames.LastName
                        && z.Attribute.Value.Contains(lastName))
                    .Select(z => z.Customer);
            }

            int beginIndex = 0, length = 0;
            string dateOfBirthStr = null;
            // ngày sinh đc lưu giữ vào database theo chuỗi ở dạng YYYY-MM-DD, vậy nên chúng ta sẽ search chúng theo dạng chuỗi này
            if(dayOfBirth > 0)
            {
                if(monthOfBirth > 0)
                {
                    // cả 2 giá trị đều đc chỉ định
                    dateOfBirthStr = monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-" + dayOfBirth.ToString("00", CultureInfo.InvariantCulture);
                    beginIndex = 5;
                    length = 5;
                }else
                {
                    dateOfBirthStr = dayOfBirth.ToString("00", CultureInfo.InvariantCulture);
                    beginIndex = 8;
                    length = 2;
                }
            }else if(monthOfBirth > 0)
            {
                dateOfBirthStr = monthOfBirth.ToString("00", CultureInfo.InvariantCulture);
                beginIndex = 5;
                length = 2;
            }

            if (beginIndex > 0)
            {
                // EndsWith ko đc hỗ trợ bởi sql ce
                // vậy nên chúng ta đi vòng như thế này http://social.msdn.microsoft.com/Forums/is/sqlce/thread/0f810be1-2132-4c59-b9ae-8f7013c0cc00
                // chúng ta cũng ko thể sử dụng hàm Length trong sql ce ( ko hỗ trợ trong ngữ cảnh này )
                query = query.Join(_genericAttributeRepository.Table, x => x.Id, y => y.EntityId,
                        (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == "Customer"
                        && z.Attribute.Key == SystemCustomerAttributeNames.DateOfBirth
                        && z.Attribute.Value.Substring(beginIndex, length) == dateOfBirthStr)
                    .Select(z => z.Customer);
            }
            if (!string.IsNullOrWhiteSpace(company))
            {
                query = query.Join(_genericAttributeRepository.Table, x => x.Id, y => y.EntityId,
                        (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == "Customer"
                        && z.Attribute.Key == SystemCustomerAttributeNames.Company
                        && z.Attribute.Value.Contains(company))
                    .Select(z => z.Customer);
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query.Join(_genericAttributeRepository.Table, x => x.Id, y => y.EntityId,
                        (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == "Customer"
                        && z.Attribute.Key == SystemCustomerAttributeNames.Phone
                        && z.Attribute.Value.Contains(phone))
                    .Select(z => z.Customer);
            }
            if (!string.IsNullOrWhiteSpace(zipPostalCode))
            {
                query = query.Join(_genericAttributeRepository.Table, x => x.Id, y => y.EntityId,
                        (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == "Customer"
                        && z.Attribute.Key == SystemCustomerAttributeNames.ZipPostalCode
                        && z.Attribute.Value.Contains(zipPostalCode))
                    .Select(z => z.Customer);
            }
            // chỉ tìm những khách hàng có giỏ hàng ( dạng wish list, shopping cart hoặc tùy ý 1 trong 2 dạng )
            if (loadOnlyWithShoppingCart)
            {
                if (sct.HasValue)
                {
                    int id = (int)sct.Value;
                    query = query.Where(x => x.ShoppingCartItems.Any(s => s.ShoppingCartTypeId == id));
                }
                else query = query.Where(x => x.ShoppingCartItems.Any());
            }
            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return new PagedList<Customer>(query, pageIndex, pageSize);
        }

        public virtual IList<Customer> GetAllCustomersByPasswordFormat(PasswordFormat passwordFormat)
        {
            int passwordFormatId = (int)passwordFormat;

            return _repository.Table
                .Where(c => c.PasswordFormatId == passwordFormatId)
                .OrderByDescending(c => c.CreatedOnUtc)
                .ToList();
        }

        public virtual IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc, int[] customerRoleIds, 
            int pageIndex, int pageSize)
        {
            var query = _repository.Table
                .Where(c => !c.Deleted && c.LastActivityDateUtc >= lastActivityFromUtc);
            if (customerRoleIds != null && customerRoleIds.Length > 0)
                query = query.Where(c => c.CustomerRoles.Select(r => r.Id).Intersect(customerRoleIds).Any());
            query = query.OrderByDescending(c => c.LastActivityDateUtc);

            return new PagedList<Customer>(query, pageIndex, pageSize);
        }

        public virtual void DeleteCustomer(Customer customer, bool publishEvent = true)
        {
            if (customer == null) throw new ArgumentNullException("customer");
            if (customer.Id <= 0) throw new ResearchException("Customer to delete must have a valid Id");
            if (customer.IsSystemAccount)
                throw new ResearchException(string.Format("System customer account ({0}) could not be deleted", customer.SystemName));

            customer.Deleted = true;

            // nếu cấu hình yêu cầu thêm hậu tố -DELETED vào cuối Email, Username thì ta sẽ thêm vào
            if(_customerSettings.SuffixDeletedCustomers)
            {
                if (!string.IsNullOrEmpty(customer.Email)) customer.Email += "-DELETED";
                if (!string.IsNullOrEmpty(customer.Username)) customer.Username += "-DELETED";
            }
            UpdateCustomer(customer, publishEvent);
        }

        public virtual IList<Customer> GetCustomersByIds(int[] customerIds)
        {
            if (customerIds == null || customerIds.Length == 0) return new List<Customer>();

            var list = _repository.Table.Where(c => customerIds.Contains(c.Id))
                .ToList();
            var result = new List<Customer>();
            if (list.Count > 0)
            {
                foreach (int id in customerIds)
                    foreach (var customer in list)
                        if (customer.Id == id)
                        {
                            result.Add(customer);
                            break;
                        }
            }
            return result;
        }

        public virtual Customer GetCustomerByGuid(Guid customerGuid)
        {
            if (customerGuid == Guid.Empty) return null;

            return _cacheWriter.GetByGuid(customerGuid, () => {
                return _repository.Table
                    .Where(p => p.CustomerGuid == customerGuid)
                    .OrderBy(p => p.Id) // câu hỏi: liệu có xảy ra chuyện 2 customer trùng guid. Liệu có cần order by ở đây ko ?
                    .FirstOrDefault();
            });
        }

        public virtual Customer GetCustomerByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            return _cacheWriter.GetByEmail(email, () =>
            {
                return _repository.Table
                    .Where(p => p.Email == email)
                    .OrderBy(p => p.Id) // câu hỏi: liệu có xảy ra chuyện 2 customer trùng email. Liệu có cần order by ở đây ko ?
                    .FirstOrDefault();
            });
        }

        public virtual Customer GetCustomerBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName)) return null;

            return _cacheWriter.GetBySystemName(systemName, () =>
            {
                return _repository.Table
                    .Where(p => p.SystemName == systemName)
                    .OrderBy(p => p.Id) // câu hỏi: liệu có xảy ra chuyện 2 customer trùng systemName. Liệu có cần order by ở đây ko ?
                    .FirstOrDefault();
            });
        }

        public virtual Customer GetCustomerByUsername(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return null;

            return _cacheWriter.GetByUsername(userName, () =>
            {
                return _repository.Table
                    .Where(p => p.Username == userName)
                    .OrderBy(p => p.Id) // câu hỏi: liệu có xảy ra chuyện 2 customer trùng userName. Liệu có cần order by ở đây ko ?
                    .FirstOrDefault();
            });
        }

        public virtual Customer InsertGuestCustomer()
        {
            // 1 guest sẽ chỉ có các thông tin bao gồm: 1 mã id duy nhất tự tăng, 1 mã guid nhận diện phát sinh tự động,
            // trạng thái active, ngày giờ tạo, ngày giờ của lần hoạt động cuối
            // họ sẽ ko có những thông tin như email, username
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow
            };

            // đồng thời phải add tài khoản của họ vào vai trò Guest, để họ có thể có đc những quyền thuộc về vai trò này
            var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
            if (guestRole == null) throw new ResearchException("'Guests' role could not be loaded");

            customer.CustomerRoles.Add(guestRole);
            _repository.Insert(customer);
            _unitOfWork.SaveChanges();

            _eventPublisher.EntityInserted(customer);
            return customer;
        }

        public virtual void ResetCheckoutData(Customer customer, int storeId, bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = false, bool clearShippingMethod = true, bool clearPaymentMethod = true)
        {
            if (customer == null) throw new ArgumentNullException("customer");
            if (storeId < 0) throw new ArgumentException("storeId must valid");

            // xem lại phần này: Các property DiscountCouponCode, GiftCardCouponCodes dường như ko phải là đối tượng ShippingOption, mà 
            // được đối xử như 1 dạng string xml. Sở dĩ thao tác Save ở đây ko có lỗi là do nó Save 1 giá trị null ( genericAttribute
            // ứng xử như nhau với giá trị null bất kể kiểu đối tượng, nó sẽ lưu trữ 1 chuỗi rỗng xuống database để biểu diễn cho null

            //clear entered coupon codes
            if(clearCouponCodes)
            {
                // hiện tại thì 2 property này chỉ lưu dang chung stroeId=0, ko lưu riêng cho từng storeId nên chỉ clear thế này
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.DiscountCouponCode, null, saveChange: false);
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, null, saveChange: false);
            }

            //clear checkout attributes
            if(clearCheckoutAttributes)
            {
                // khác ở đây
                _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.CheckoutAttributes, null, storeId, saveChange: false);
            }
            //clear reward points flag
            if (clearRewardPoints)
            {
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, false, storeId, saveChange: false);
            }
            //clear selected shipping method
            if (clearShippingMethod)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.SelectedShippingOption, null, storeId, saveChange: false);
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.OfferedShippingOptions, null, storeId, saveChange: false);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedPickUpInStore, false, storeId, saveChange: false);
            }

            //clear selected payment method
            if (clearPaymentMethod)
            {
                _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, storeId, saveChange: false);
            }

            _genericAttributeService.SaveChange(false);

            var event1 = new EntityAllChange<GenericAttribute>(null);
            var event2 = new EntityUpdated<Customer>(customer);
            _eventPublisher.Publish(event1, event2); // phát ra sự kiện thay đổi trên genericAtrribute và 1 yêu cầu cập nhật trên
            // customer ( do các property tương ứng với nó trong generic attribute bị thay đổi )
        }

        public virtual int DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, DateTime? lastActivityDateUtcFrom,
            DateTime? lastActivityDateUtcTo, bool onlyWithoutShoppingCart)
        {
            int totalRecordsDeleted = _customerRepository.DeleteGuestCustomers(createdFromUtc, createdToUtc,
                lastActivityDateUtcFrom, lastActivityDateUtcTo, onlyWithoutShoppingCart);
            if (totalRecordsDeleted > 0)
            {
                // phát sinh sự kiện và clear cache
                var event1 = new EntityAllChange<GenericAttribute>(null);
                var event2 = new EntityAllChange<Customer>(null);
                _eventPublisher.Publish(event1, event2);
            }
            return totalRecordsDeleted;
        }

        public virtual void InsertCustomer(Customer entity, bool publishEvent = true)
        {
            Insert(entity, true, publishEvent);
        }

        public virtual void UpdateCustomer(Customer entity, bool publishEvent = true)
        {
            Update(entity, true, publishEvent);
        }

        #endregion


        #region Roles

        public virtual void DeleteCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null) throw new ArgumentNullException("customerRole");
            if (customerRole.IsSystemRole) throw new ResearchException("Không thể xóa SystemRole");

            _customerRoleRepository.Delete(customerRole);
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityDeleted(customerRole);
        }

        public virtual CustomerRole GetCustomerRoleById(int id)
        {
            if (id <= 0) return null;
            return _cacheWriter.GetCustomerRoleById(id, () => _customerRoleRepository.GetById(id));
        }

        public virtual CustomerRole GetCustomerRoleBySystemName(string systemName)
        {
            if (string.IsNullOrEmpty(systemName)) return null;

            return _cacheWriter.GetCustomerRoleBySystemName(systemName, () => {
                return _customerRoleRepository.Table.Where(p => p.SystemName == systemName).FirstOrDefault();
            });
        }

        public virtual IList<CustomerRole> GetAllCustomerRoles(bool showHidden = false)
        {
            return _cacheWriter.GetAllCustomerRole(showHidden, () => {
                var query = _customerRoleRepository.Table;
                if (!showHidden) query = query.Where(p => p.Active);
                return query.OrderBy(p => p.Name).ToList();
            });
        }

        public virtual CustomerRole InsertCustomerRole(CustomerRole customerRole)
        {
            var result = _customerRoleRepository.Insert(customerRole);
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityInserted(customerRole);
            return result;
        }

        public virtual void UpdateCustomerRole(CustomerRole customerRole)
        {
            _customerRoleRepository.Update(customerRole);
            _unitOfWork.SaveChanges();
            if (_eventPublisher != null) _eventPublisher.EntityUpdated(customerRole);
        }
        
        #endregion

        #endregion

    }
}
