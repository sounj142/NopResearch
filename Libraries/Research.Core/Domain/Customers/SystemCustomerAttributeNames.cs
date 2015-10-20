

namespace Research.Core.Domain.Customers
{
    /// <summary>
    /// Lớp khai báo các property bổ sung mà Customer định nghĩa thêm, được lưu trữ vào trong bảng chung GenericAttribute
    /// </summary>
    public static partial class SystemCustomerAttributeNames
    {
        /// <summary>
        /// Property cho biết là đã áp dụng kỹ thuật tự động phát hiện ngôn ngữ dựa trên cấu hình trình duyệt cho customer này hay chưa,
        /// nếu đã dùng thì giá trị này là true và sẽ ko áp dụng kỹ thuật này nữa -- 1 customer chỉ đc áp dụng kỹ thuật này duy nhất 1
        /// lần trong đời ???
        /// </summary>
        public const string LanguageAutomaticallyDetected = "LanguageAutomaticallyDetected";

        /// <summary>
        /// Lưu ngôn ngữ hiện hành của người dùng. Bằng cách này, ngôn ngữ của người dùng có thể được lưu giữ trường tồn bất kể 
        /// session, bất kể trình duyêt, bất kể cookies. Chỉ cần người dùng đăng nhập là có thể khôi phục lại với ngôn ngữ mà họ
        /// dùng trong lần trước đó ?
        /// Ngôn ngữ này sẽ chỉ được dùng trong giao diện khách hàng
        /// </summary>
        public const string LanguageId = "LanguageId";

        /// <summary>
        /// Ngôn ngữ dùng cho giao diện admin
        /// </summary>
        public const string AdminLanguageId = "AdminLanguageId";

        /// <summary>
        /// Id của tài khoản đại diện cho tài khoản Customer hiện hành ( Xem IWebContext.CurrentCustomer )
        /// </summary>
        public const string ImpersonatedCustomerId = "ImpersonatedCustomerId";

        /// <summary>
        /// mã tiền tệ hiện hành của customer
        /// </summary>
        public const string CurrencyId = "CurrencyId";

        /// <summary>
        /// kiểu hiển thị thuế hiện hành ???
        /// </summary>
        public const string TaxDisplayTypeId = "TaxDisplayTypeId";

        /// <summary>
        /// First name của customer. First name ko hề đc lưu trong bảng Customer mà nó đc lưu trong GenericAttribute
        /// </summary>
        public const string FirstName = "FirstName";

        public const string LastName = "LastName";

        public const string Company = "Company";

        public const string DateOfBirth = "DateOfBirth";

        public const string Phone = "Phone";

        public const string ZipPostalCode = "ZipPostalCode";

        public const string DiscountCouponCode = "DiscountCouponCode";

        public const string GiftCardCouponCodes = "GiftCardCouponCodes";

        public const string CheckoutAttributes = "CheckoutAttributes";

        public const string UseRewardPointsDuringCheckout = "UseRewardPointsDuringCheckout";

        public const string SelectedShippingOption = "SelectedShippingOption";

        public const string OfferedShippingOptions = "OfferedShippingOptions";

        public const string SelectedPickUpInStore = "SelectedPickUpInStore";

        public const string SelectedPaymentMethod = "SelectedPaymentMethod";

        public const string TimeZoneId = "TimeZoneId";
    }
}
