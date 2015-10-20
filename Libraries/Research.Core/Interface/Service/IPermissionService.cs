using Research.Core.Domain.Customers;
using Research.Core.Domain.Security;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// service quản lý các quyền con.
    /// Tên quyền con cũng được hỗ trợ hiển thị đa ngôn ngữ, nhưng nó được chứa trong bảng các chuỗi dịch tài nguyên thông thường 
    /// LocaleStringResource chứ ko phải là đa ngôn ngữ như là các property của Product
    /// </summary>
    public partial interface IPermissionService
    {
        void Delete(PermissionRecord entity);

        PermissionRecord GetById(int id);

        PermissionRecord GetPermissionRecordBySystemName(string systemName);

        IList<PermissionRecord> GetAllPermissionRecords();

        void Insert(PermissionRecord entity);

        void Update(PermissionRecord entity);

        /// <summary>
        /// Hàm chịu trách nhiệm ghi các quyền ban đầu xuống database trong bước khởi tạo database lúc chạy Nop lần đầu tiên.
        /// Thao tác ghi bao gồm có ghi tất cả các PermissionRecord trong permissionProvider.GetPermissions(), và ghi tất cả các role
        /// và ánh xạ permission-role trong permissionProvider.GetDefaultPermissions()
        /// </summary>
        void InstallPermissions(IPermissionProvider permissionProvider);

        /// <summary>
        /// Uninstall permissions. Cho phép xóa bỏ chỉ duy nhất các PermissionRecord permissionProvider.GetPermissions()
        /// </summary>
        void UninstallPermissions(IPermissionProvider permissionProvider);

        /// <summary>
        /// Kiểm tra xem người dùng hiện hành có quyền permission hay ko. Thao tác gọi lại static cache
        /// </summary>
        bool Authorize(PermissionRecord permission);

        /// <summary>
        /// Kiểm tra xem người dùng customer có quyền permission hay ko. Thao tác gọi lại static cache
        /// </summary>
        bool Authorize(PermissionRecord permission, Customer customer);

        /// <summary>
        /// Kiểm tra xem người dùng hiện hành có quyền với system name là permissionRecordSystemName hay ko. Thao tác gọi lại static cache
        /// </summary>
        bool Authorize(string permissionRecordSystemName);

        /// <summary>
        /// Kiểm tra xem người dùng customer có quyền với system name là permissionRecordSystemName hay ko. Thao tác gọi lại static cache
        /// </summary>
        bool Authorize(string permissionRecordSystemName, Customer customer);
    }
}
