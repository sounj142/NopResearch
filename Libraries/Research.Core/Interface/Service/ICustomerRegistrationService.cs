using Research.Core.Domain.Customers;
using Research.Core.Domain.Customers.ViewModels;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// interface thực hiện các thao tác kiểm tra cho chức năng đăng ký, đăng nhập. Các hàm ở service này sẽ gọi lại CustomerService,
    /// update database và publish event
    /// </summary>
    public partial interface ICustomerRegistrationService
    {
        /// <summary>
        /// Kiểm tra xem tên đăng nhập và mật khẩu có khớp với 1 tài khoản trong database hay ko.
        /// Ở cuối phần kiểm tra, hàm tự động lưu lại giá trị mới cho lần đăng nhập gần nhất LastLoginDateUtc, đồng thời save change và
        /// phát sinh sự kiên cập nhật customer
        /// --- cần phải đảm bảo chỉ gọi hàm này khi thực sự thực hiện thao tác đăng nhập
        /// </summary>
        CustomerLoginResults ValidateCustomer(string usernameOrEmail, string password);

        /// <summary>
        /// Đăng ký tài khoản mới, về bản chất đây là thao tác bổ sung thêm thông tin như username, email, pass vào tài khoản guest đã có 
        /// request.Customer, sau đó chuyển tài khoản này sang role Registerd.
        /// Ghi chú là hàm sẽ insert database và publish event
        /// </summary>
        CustomerRegistrationResult RegisterCustomer(CustomerRegistrationRequest request);

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        PasswordChangeResult ChangePassword(ChangePasswordRequest request);

        /// <summary>
        /// Thiết lập 1 email mới cho tài khoản
        /// </summary>
        void SetEmail(Customer customer, string newEmail);

        /// <summary>
        /// Thiết lập 1 username mới cho tài khoản
        /// </summary>
        void SetUsername(Customer customer, string newUsername);
    }
}
