using Research.Core.Domain.Customers;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Service chịu trách nhiệm ghi/xóa các thông tin đăng nhập cho người dùng khi họ đăng nhập/đăng xuất
    /// Ghi chú là service chỉ chịu trách nhiệm ghi các thông tin đăng nhập kiểu như cookies, việc xác thực thuộc trách nhiệm của code khác
    /// 
    /// Hệ thống sẽ duy trì tình trạng đăng nhập của người dùng dựa trên giá trị cookies. Trong cookies này ta sẽ lưu trữ cả email và
    /// password, dùng để kiểm tra lại mỗi lần đọc lại dữ liệu người dùng từ database lên
    /// 
    /// Mỗi lần lấy dữ liệu Customer từ database, tình trạng tài khoản người dùng (tồn tại/ko ; active/unactive; mật khẩu đúng/ko đúng)
    /// sẽ đc kiểm tra. Nếu ko so khớp thì tùy trường hợp sẽ có xử lý phù hợp, thường thì sẽ tiến hành xóa cookies và cưỡng ép logout
    /// </summary>
    public partial interface IAuthenticationService
    {
        /// <summary>
        /// Ghi cookies đăng nhập cho người dùng customer, ghi lâu dài nếu có yêu cầu ( tương ứng với phần cuối thao tác login sau các kiểm
        /// tra tài khoản hợp lệ)
        /// 
        /// Hàm chỉ được phép gọi trong ngữ cảnh Request bình thường, ko được gọi trong ngữ cảnh Task khi mà request ko tồn tại
        /// </summary>
        void SignIn(Customer customer, bool createPersistentCookie);

        /// <summary>
        /// Đăng xuất người dùng hiện hành, Xóa cookies và các thông tin liên quan ( tương ứng với thao tác logout )
        /// 
        /// Hàm chỉ được phép gọi trong ngữ cảnh Request bình thường, ko được gọi trong ngữ cảnh Task khi mà request ko tồn tại
        /// </summary>
        void SignOut();

        /// <summary>
        /// Lấy ra thông tin người dùng từ dữ liệu cookies. Thông qua dữ liệu cookies, đọc và lấy ra người dùng từ database. Tiến
        /// hành xác minh lại trạng thái người dùng, nếu người dùng đã bị xóa, bị ban
        /// 
        /// Hàm có thể gọi trong ngữ cảnh bất kỳ, kể cả ngữ cảnh task, sẽ tự kiểm tra để có xử lý phù hợp
        /// </summary>
        Customer GetAuthenticatedCustomer();
    }
}
