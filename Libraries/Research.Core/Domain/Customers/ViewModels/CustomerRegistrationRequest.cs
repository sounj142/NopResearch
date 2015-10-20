

namespace Research.Core.Domain.Customers.ViewModels
{
    /// <summary>
    /// Lớp chứa các thông tin đầu vào cho hàm đăng ký Customer mới
    /// </summary>
    public class CustomerRegistrationRequest
    {
        /// <summary>
        /// Customer đóng vai trò tài khoản Guest mà khách hàng đang xài, thao tác đăng ký nếu thành công sẽ bổ sung các thông tin cần thiết
        /// vào tài khoản này và chuyển nó sang vai trò Registered
        /// </summary>
        public Customer Customer { get; set; }

        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public PasswordFormat PasswordFormat { get; set; }
        public bool IsApproved { get; set; }

        public CustomerRegistrationRequest(Customer customer, string email, string username,
            string password,
            PasswordFormat passwordFormat,
            bool isApproved = true)
        {
            this.Customer = customer;
            this.Email = email;
            this.Username = username;
            this.Password = password;
            this.PasswordFormat = passwordFormat;
            this.IsApproved = isApproved;
        }
    }
}
