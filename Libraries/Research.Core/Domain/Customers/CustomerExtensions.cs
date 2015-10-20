using System;
using System.Linq;
using Research.Core.Domain.Common;
using Research.Core.Domain.Orders;
using Research.Core.Interface.Service;
using Research.Core.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace Research.Core.Domain.Customers
{
    public static class CustomerExtensions
    {
        #region Customer role

        /// <summary>
        /// Kiểm tra xem 1 customer có thuộc về 1 role nào đó hay ko
        /// </summary>
        public static bool IsInCustomerRole(this Customer customer, string customerRoleSystemName,
            bool onlyActiveCustomerRoles = true)
        {
            if (string.IsNullOrEmpty(customerRoleSystemName)) 
                throw new ArgumentNullException("customerRoleSystemName");

            return customer.CustomerRoles.Any(p => (!onlyActiveCustomerRoles || p.Active) && 
                string.Equals(p.SystemName, customerRoleSystemName, StringComparison.InvariantCulture));
        }

        /// <summary>
        /// Cho biết 1 tài khoản có phải là tài khoản dành riêng cho Search engine hay ko. Toàn hệ thống chỉ có 1 tài khoản loại này
        /// , bất kỳ request đến nào ko có cookies cho Register/Guest user và đc kiểm tra là search enginer đều được tự động gán
        /// cho sử dụng tài khoản này
        /// </summary>
        public static bool IsSearchEngineAccount(this Customer customer)
        {
            return customer.IsSystemAccount &&
                SystemCustomerNames.SearchEngine.Equals(customer.SystemName, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Cho biết customer có phải là tai khoản dành riêng cho background task hay ko
        /// </summary>
        public static bool IsBackgroundTaskAccount(this Customer customer)
        {
            return customer.IsSystemAccount &&
                SystemCustomerNames.BackgroundTask.Equals(customer.SystemName, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra xem 1 customer có thuộc về 1 role Administrators hay ko
        /// </summary>
        public static bool IsAdmin(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Administrators, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Kiểm tra xem 1 customer có thuộc về 1 role ForumModerators hay ko
        /// </summary>
        public static bool IsForumModerator(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.ForumModerators, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Kiểm tra xem 1 customer có thuộc về 1 role Registered ( đã đăng ký tài khoản ) hay ko
        /// </summary>
        public static bool IsRegistered(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Registered, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Kiểm tra xem 1 customer có thuộc về 1 role Guests hay ko
        /// </summary>
        public static bool IsGuest(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Guests, onlyActiveCustomerRoles);
        }

        /// <summary>
        /// Kiểm tra xem 1 customer có thuộc về 1 role Vendors hay ko
        /// </summary>
        public static bool IsVendor(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Vendors, onlyActiveCustomerRoles);
        }

        #endregion

        #region Customer extension //// Có thể xem xét cache per request các hàm trong này để tăng tốc ???

        /// <summary>
        /// Lấy tên đầy đủ của customer( dữ liệu từ generic attribute ).
        /// </summary>
        public static string GetFullName(this Customer customer, IGenericAttributeService genericAttributeService = null)
        {
            if(genericAttributeService == null)
                genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            string firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName, genericAttributeService);
            string lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName, genericAttributeService);

            if (string.IsNullOrWhiteSpace(lastName))
                return firstName ?? string.Empty;
            else
            {
                if (string.IsNullOrWhiteSpace(firstName))
                    return lastName;
                else return string.Format("{0} {1}", firstName, lastName);
            }
        }

        /// <summary>
        /// Lấy về tên hiển thị ra giao diện cho người dùng, ( thường là "Chào, xxx" ở góc phải trên ),
        /// tùy vào cấu hình mà tên hiển thị này có thể là user name, email, full name.
        /// Cho phép tùy chọn cắt chuỗi nếu chuỗi tên dài hơn giới hạn số ký tự
        /// </summary>
        public static string FormatUserName(this Customer customer, CustomerSettings customerSettings = null,
            IGenericAttributeService genericAttributeService = null, bool stripTooLong = false, int maxLength = 0)
        {
            if (customer == null) return string.Empty;

            var engine = EngineContext.Current;
            string result = string.Empty;
            if (customer.IsGuest())
                result = engine.Resolve<ILocalizationService>().GetResource("Customer.Guest", false);
            else
            {
                if (customerSettings == null)
                    customerSettings = engine.Resolve<CustomerSettings>();
                switch (customerSettings.CustomerNameFormat)
                {
                    case CustomerNameFormat.ShowEmails:
                        result = customer.Email;
                        break;
                    case CustomerNameFormat.ShowUsernames:
                        result = customer.Username;
                        break;
                    case CustomerNameFormat.ShowFirstName:
                    case CustomerNameFormat.ShowFullNames:
                        if (genericAttributeService == null)
                            genericAttributeService = engine.Resolve<IGenericAttributeService>();
                        if (customerSettings.CustomerNameFormat == CustomerNameFormat.ShowFullNames)
                            result = customer.GetFullName(genericAttributeService);
                        else
                            result = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName, genericAttributeService);
                        break;
                }
            }
            if (result == null) result = string.Empty;
            if (stripTooLong && maxLength > 0)
                result = CommonHelper.EnsureMaximumLength(result, maxLength);
            return result;
        }

        /// <summary>
        /// Đọc dữ liệu lưu trữ trong generic attribute để lấy về danh sách các mã giảm giá ( coupon codes ) mà khách hàng này đã từng
        /// sử dụng để mua hàng. Mỗi coupon codes ấy sẽ là mã của 1 gift card
        /// </summary>
        public static IList<string> ParseAppliedGiftCardCouponCodes(this Customer customer, 
            IGenericAttributeService genericAttributeService = null)
        {
            if (genericAttributeService == null)
                genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

            string existingGiftCartCouponCodes = customer.GetAttribute<string>(SystemCustomerAttributeNames.GiftCardCouponCodes,
                genericAttributeService);
            var couponCodes = new List<string>();
            if (string.IsNullOrEmpty(existingGiftCartCouponCodes)) return couponCodes;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(existingGiftCartCouponCodes);

                foreach(XmlNode node in xmlDoc.SelectNodes(@"//GiftCardCouponCodes/CouponCode"))
                {
                    if(node.Attributes != null && node.Attributes["Code"] != null)
                    {
                        string code = node.Attributes["Code"].InnerText;
                        if (code != null) code = code.Trim();
                        if (!string.IsNullOrEmpty(code)) couponCodes.Add(code);
                    }
                }
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return couponCodes;
        }

        #endregion
    }
}
