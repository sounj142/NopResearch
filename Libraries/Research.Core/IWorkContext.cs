using Research.Core.Domain.Customers;
using Research.Core.Domain.Directory;
using Research.Core.Domain.Localization;
using Research.Core.Domain.Tax;
using Research.Core.Domain.Vendors;

namespace Research.Core
{
    /// <summary>
    /// đối tượng ngữ cảnh làm việc ? Về bản chất nó thuộc cùng tầng với các IService
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets or sets the current customer
        /// </summary>
        Customer CurrentCustomer { get; set; }

        /// <summary>
        /// Gets or sets the original customer (in case the current one is impersonated).
        /// hiểu nôm na là 1 tài khoản sử dụng 1 tài khoản khác làm đại diện cho nó. 1 tài khoản customer có thể khai báo 1 property
        /// tên ImpersonatedCustomerId trong GenericAttribute để chứa Id của 1 tài khoản khác. Và tài khoản tương ứng với 
        /// </summary>
        Customer OriginalCustomerIfImpersonated { get; }

        /// <summary>
        /// Gets the current vendor (logged-in manager).
        /// Trả về đối tượng vendor liên kết với customer hiện hành nếu có
        /// </summary>
        Vendor CurrentVendor { get; }

        /// <summary>
        /// Lấy hoặc thiết lập ngôn ngữ hiện hành của người dùng ( ngôn ngữ được lấy ra từ url/cấu hình browser(1 lần duy nhất cho mối customer)/
        /// hoặc thông tin generic attribute của current customer ). Ngôn ngữ hiện hành được cache lại để đảm bảo rằng nó sẽ chỉ được
        /// lấy 1 lần. 
        /// Thao tác set cho phép thiết lập lại ngôn ngữ hiện hành có kiểm tra điều kiện ngôn ngữ đc store cho phép dùng
        /// 
        /// Ngay cả trường hợp store ko hỗ trợ ngôn ngữ nào, hệ thống cũng trả về ngôn ngữ đầu tiên để xài tạm. Sẽ chỉ có vấn đề
        /// khi hệ thống ko có ngôn ngữ nào cả
        /// </summary>
        Language WorkingLanguage { get; set; }

        /// <summary>
        /// Lấy hoặc thiết lập tiền tệ hiện hành
        /// </summary>
        Currency WorkingCurrency { get; set; }
        

        /// <summary>
        /// Get or set value indicating whether we're in admin area
        /// Trả về true nếu request hiện hành thuộc về giao diện quản trị admin ( cách xử lý ở giao diện quản trị và font-end là khác nhau )
        /// </summary>
        bool IsAdmin { get; set; }

        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        TaxDisplayType TaxDisplayType { get; set; }
    }
}
