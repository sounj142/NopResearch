using System.Collections.Generic;
using Research.Core.Domain.Security;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Permission provider.
    /// Cung cấp danh sách các permision đang có của hệ thống
    /// </summary>
    public interface IPermissionProvider
    {
        /// <summary>
        /// Danh sách tất cả các permision đang có của hệ thống, cũng dùng chủ yếu cho chức năng install database ???
        /// </summary>
        IEnumerable<PermissionRecord> GetPermissions();

        /// <summary>
        /// Lấy về danh sách các DefaultPermissionRecord, mỗi phần tử DefaultPermissionRecord chứa 1 list các PermissionRecord và
        /// có 1 tên CustomerRoleSystemName. Chỉ Được sử dụng để định nghĩa nhóm các quyền con mặc định ban đầu của từng CustomerRole,
        /// dùng cho chức năng sinh dữ liệu mẫu ở thời điểm ban đầu khi chưa có database, chứ ko có ý nghĩa sử dụng sau này
        /// </summary>
        IEnumerable<DefaultPermissionRecord> GetDefaultPermissions();
    }
}
