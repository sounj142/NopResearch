
namespace Research.Core.Domain.Customers.ViewModels
{
    /// <summary>
    /// Lớp chứa các thông tin đầu vào dùng cho yêu cầu đổi password
    /// </summary>
    public class ChangePasswordRequest
    {
        public string Email { get; set; }
        
        /// <summary>
        /// true nếu muốn kiểm tra old password trùng với password đang có đồng thời tài khoản phải ko bị xóa và active, 
        /// false nếu ko muốn kiểm tra bất của điều kiện gì ở trên. Truyền vào false nếu muốn đổi pass ở giao diện admin ?
        /// </summary>
        public bool ValidateRequest { get; set; }
        public PasswordFormat NewPasswordFormat { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }

        public ChangePasswordRequest(string email, bool validateRequest, PasswordFormat newPasswordFormat,
            string newPassword, string oldPassword = "")
        {
            this.Email = email;
            this.ValidateRequest = validateRequest;
            this.NewPasswordFormat = newPasswordFormat;
            this.NewPassword = newPassword;
            this.OldPassword = oldPassword;
        }
    }
}
