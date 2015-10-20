using Research.Core.Domain.Customers;
using Research.Core.Domain.Security;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// bảng AclRecord cho phép ràng buộc về quyền ở 1 vài đối tượng entity cụ thể nào đó. Nó là tách biệt với cơ chế quyền con 
    /// PermissionRecord áp dụng cho chức năng admin. Thường thì nó được áp dụng để đánh dấu 1 sản phẩm nào đó là "chưa được publish"
    /// 1 sản phẩm đc đánh dấu là chưa được publish khi cờ SubjectToAcl được bật là true, và ko có bất cứ Role nào đc ánh xạ đến nó
    /// trong bảng AclRecord. Khi đó, các chức năng như mua hàng sẽ từ chối thanh toán sản phẩm
    /// 
    /// </summary>
    public partial interface IAclService
    {
        void Delete(AclRecord entity);

        AclRecord GetById(int id);

        /// <summary>
        /// Lấy về những trường AclRecord liên kết với đối tượng entity. Chính xác hơn là sẽ lấy về danh sách CustomerRoleId
        /// liên kết với entity được mô tả trong bảng AclRecord
        /// </summary>
        IList<AclRecord> GetAclRecords<T>(T entity) where T : BaseEntity, IAclSupported;

        void Insert(AclRecord entity);

        /// <summary>
        /// Chèn 1 dòng vào AclRecord, mô tả ánh xạ giữa entity và customerRoleId.
        /// Để hàm này tạo ra dữ liệu chính xác, trước khi gọi hàm cần kiểm tra để đảm bảo trong bảng AclRecord ko có bất cứ dòng nào
        /// ánh xạ entity - customerRoleId
        /// </summary>
        void InsertAclRecord<T>(T entity, int customerRoleId) where T : BaseEntity, IAclSupported;

        void Update(AclRecord entity);

        /// <summary>
        /// Lấy về danh sách các CustomerRoleId liên kết với entity được mô tả trong bảng AclRecord
        /// Chính xác thì nên được hiểu là lấy về những CustomerRole được phép tương tác sửa đổi đối tượng entity
        /// . Kết quả Hàm này đc cache static để tránh phải gọi lại nhiều lần trong các thao tác kiểm tra quyền sau này
        /// </summary>
        IList<int> GetCustomerRoleIdsWithAccess<T>(T entity) where T : BaseEntity, IAclSupported;

        /// <summary>
        /// Kiểm tra xem theo dữ liệu trong bảng AclRecord, người dùng hiện hành có quyền tương tác trên đối tượng entity hay ko.
        /// Hàm sẽ lấy ra danh sách CustomerRole của người dùng hiện hành, và kiểm tra sự tồn tại của nó trong danh sách
        /// GetCustomerRoleIdsWithAccess(entity)
        /// </summary>
        bool Authorize<T>(T entity) where T : BaseEntity, IAclSupported;

        /// <summary>
        /// Kiểm tra xem theo dữ liệu trong bảng AclRecord, người dùng customer có quyền tương tác với entity hay ko
        /// </summary>
        bool Authorize<T>(T entity, Customer customer) where T : BaseEntity, IAclSupported;
    }
}
