using Research.Core.Domain.Customers;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    /// <summary>
    /// OK, chúng ta sẽ chỉ cache cho Customer và CustomerRole ở mức per request, bởi vì đây ko phải là những class thường chỉ đc truy cập
    /// trực tiếp trong giao diện quản lý
    /// + Mối quan hệ mà chúng ta cần bận tâm nhất là mối quan hệ xa Customer - PermissionRecord thì đã đc cache static ổn thỏa trong
    /// PermissionService
    /// + Vấn đề lưu giữ người dùng hiện hành: Ta có thể mở rộng User.Identity của FormAuthentication để lưu email, đồng thời
    /// lưu thêm password hiện hành. Ngoài ra còn có thể lưu 1 mã Guid vào đây ( đề cập ở dưới )
    /// 
    /// 
    /// Điểm cốt yếu là ở trong cache static, ta duy trì 1 khóa Guid. Khóa này là version của Customer. Bất kỳ thao tác nào có thể
    /// gây ra thay đổi về customer, thay đổi về quyền, ...v.v... cần phải đọc lại dữ liệu từ database thì ta sẽ gán cho khóa
    /// này 1 giá trị guid mới.
    /// Mỗi khi truy cập vào user đc lưu trữ trong session hoặc user đc lưu trong User.Identity, sẽ có 1 code tiền xử lý kiểm tra lại
    /// khóa guid trong cache. Nếu khóa Guid ko đổi thì dữ liệu trong Session/User.Identity được lấy ra xài. Nếu Guild bị thay đổi thì
    /// 1 thao tác đọc database sẽ đc tiến hành để đọc dữ liệu mới lên, thực hiện các kiểm tra cần thiết với tài khoản hiện hành.
    /// Nếu vẫn ok thì thay thế data mới vào và chạy, nếu ko ok ( chẳng hạn khi tài khoản bị banned ) thì sẽ tiến hành clear và logout
    /// ==> Tuy nhiên phương án này có hạn chế là phạm vi ảnh hưởng quá rộng, cứ ở đâu đó có 1 chút nhúc nhích gì đó như là ai đó đổi pass
    /// thì mã Guid lập tức thay đổi và TẤT CẢ người dùng đang online hiện hành đều cần phải cấp nhật
    /// 
    /// 
    /// 
    /// ==> Sau khi nghiên cứu thì kết luận là ta sẽ vẫn dùng phương án đọc lại User duy nhất 1 lần ở mỗi request, cách này đơn giản mà
    /// chính xác nhất
    /// </summary>
    public interface ICustomerAndRoleCacheWriter
    {
        #region Customer 

        /// <summary>
        /// Lấy customer theo id, cache per request
        /// </summary>
        Customer GetById(int id, Func<Customer> acquire);

        /// <summary>
        /// Lấy customer theo email, cache per request
        /// </summary>
        Customer GetByEmail(string email, Func<Customer> acquire);

        /// <summary>
        /// Lấy customer theo guid, cache per request
        /// </summary>
        Customer GetByGuid(Guid guid, Func<Customer> acquire);

        /// <summary>
        /// Lấy customer theo systemName, cache per request
        /// </summary>
        Customer GetBySystemName(string systemName, Func<Customer> acquire);

        /// <summary>
        /// Lấy customer theo username, cache per request
        /// </summary>
        Customer GetByUsername(string username, Func<Customer> acquire);

        #endregion

        #region Customer Role

        /// <summary>
        /// Lấy CustomerRole theo id, cache per request
        /// </summary>
        CustomerRole GetCustomerRoleById(int id, Func<CustomerRole> acquire);

        /// <summary>
        /// Lấy CustomerRole theo id, cache per request
        /// </summary>
        CustomerRole GetCustomerRoleBySystemName(string systemName, Func<CustomerRole> acquire);

        /// <summary>
        /// Lấy các CustomerRole theo cờ showHidden, cache per request
        /// </summary>
        IList<CustomerRole> GetAllCustomerRole(bool showHidden, Func<IList<CustomerRole>> acquire);



        #endregion
    }
}
