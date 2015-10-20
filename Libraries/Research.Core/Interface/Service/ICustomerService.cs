using System;
using System.Collections.Generic;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Orders;

namespace Research.Core.Interface.Service
{
    public partial interface ICustomerService
    {
        #region Customers

        /// <summary>
        /// Tìm các customer theo các tiêu chí tìm kiếm cho trước
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="customerRoleIds">A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="email">Email; null to load all customers</param>
        /// <param name="username">Username; null to load all customers</param>
        /// <param name="firstName">First name; null to load all customers</param>
        /// <param name="lastName">Last name; null to load all customers</param>
        /// <param name="dayOfBirth">Day of birth; 0 to load all customers</param>
        /// <param name="monthOfBirth">Month of birth; 0 to load all customers</param>
        /// <param name="company">Company; null to load all customers</param>
        /// <param name="phone">Phone; null to load all customers</param>
        /// <param name="zipPostalCode">Phone; null to load all customers</param>
        /// <param name="loadOnlyWithShoppingCart">Value indicating whther to load customers only with shopping cart</param>
        /// <param name="sct">Value indicating what shopping cart type to filter; userd when 'loadOnlyWithShoppingCart' param is 'true'</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customer collection</returns>
        IPagedList<Customer> GetAllCustomers(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int affiliateId = 0,
            int vendorId = 0, int[] customerRoleIds = null, string email = null, string username = null, string firstName = null,
            string lastName = null, int dayOfBirth = 0, int monthOfBirth = 0, string company = null, string phone = null,
            string zipPostalCode = null, bool loadOnlyWithShoppingCart = false, ShoppingCartType? sct = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Tìm tất cả các customer theo password format mà customer đó sử dụng, kết quả bao gồm cả những customer bị xóa
        /// </summary>
        /// <param name="passwordFormat">Password format</param>
        /// <returns>Customers</returns>
        IList<Customer> GetAllCustomersByPasswordFormat(PasswordFormat passwordFormat);

        /// <summary>
        /// Tìm các customer đang online tính từ 1 mốc thời gian cho trước
        /// </summary>
        /// <param name="lastActivityFromUtc">Customer last activity date (from)</param>
        /// <param name="customerRoleIds">A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customer collection</returns>
        IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc, int[] customerRoleIds, int pageIndex, int pageSize);

        /// <summary>
        /// Hàm xóa bằng cách đặt cờ xóa, ko phải xóa vật lý
        /// </summary>
        void DeleteCustomer(Customer customer, bool publishEvent = true);

        Customer GetById(int id);

        /// <summary>
        /// Lấy danh sách customer theo danh sách Id
        /// </summary>
        IList<Customer> GetCustomersByIds(int[] customerIds);

        /// <summary>
        /// Lấy về customer theo mã nhận diện Guid
        /// </summary>
        Customer GetCustomerByGuid(Guid customerGuid);

        /// <summary>
        /// Lấy customer theo email
        /// </summary>
        Customer GetCustomerByEmail(string email);

        /// <summary>
        /// Lấy customer theo systemName, systemName là 1 tên rất đặc biệt, chỉ được dùng để đặt tên cho 1 vài tài khoản đặc biệt, chẳng hạn như
        /// tài khoản dành cho máy tìm kiếm SearchEngine, tài khoản dành cho back task BackgroundTask.
        /// </summary>
        Customer GetCustomerBySystemName(string systemName);

        /// <summary>
        /// Lấy customer theo username
        /// </summary>
        Customer GetCustomerByUsername(string userName);

        /// <summary>
        /// Tạo ra và chèn vào database 1 guest customer
        /// </summary>
        Customer InsertGuestCustomer();

        void InsertCustomer(Customer entity, bool publishEvent = true);

        void UpdateCustomer(Customer entity, bool publishEvent = true);

        void Update(IList<Customer> entities);

        /// <summary>
        /// Reset các thông tin thanh toán được lưu tạm của người dùng ( thường thì đc lưu trữ trong bảng GenericAttribute )
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="clearCouponCodes">A value indicating whether to clear coupon code</param>
        /// <param name="clearCheckoutAttributes">A value indicating whether to clear selected checkout attributes</param>
        /// <param name="clearRewardPoints">A value indicating whether to clear "Use reward points" flag</param>
        /// <param name="clearShippingMethod">A value indicating whether to clear selected shipping method</param>
        /// <param name="clearPaymentMethod">A value indicating whether to clear selected payment method</param>
        void ResetCheckoutData(Customer customer, int storeId, bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = false, bool clearShippingMethod = true, bool clearPaymentMethod = true);

        /// <summary>
        /// Xóa bỏ bớt các tài khoản guest customer, cho phép loại bỏ bớt những tài khoản Guest không còn cần đến, giúp cho bảng Customer
        /// nhẹ hơn, từ đó truy cập nhanh hơn
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete customers only without shopping cart</param>
        /// <returns>Number of deleted customers</returns>
        int DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, DateTime? lastActivityDateUtcFrom,
            DateTime? lastActivityDateUtcTo, bool onlyWithoutShoppingCart);

        #endregion

        #region Roles

        /// <summary>
        /// Xóa 1 role
        /// </summary>
        void DeleteCustomerRole(CustomerRole customerRole);

        CustomerRole GetCustomerRoleById(int id);

        /// <summary>
        /// Lấy về role theo systemName. systemName là 1 tên đặc biệt dùng để nhận diện role
        /// <param name="systemName">Customer role system name</param>
        /// <returns>Customer role</returns>
        CustomerRole GetCustomerRoleBySystemName(string systemName);

        IList<CustomerRole> GetAllCustomerRoles(bool showHidden = false);

        CustomerRole InsertCustomerRole(CustomerRole customerRole);

        void UpdateCustomerRole(CustomerRole customerRole);

        #endregion
    }
}