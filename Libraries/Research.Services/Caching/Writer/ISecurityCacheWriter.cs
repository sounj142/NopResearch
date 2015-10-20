using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    /// <summary>
    /// interface chịu trách nhiệm ghi và clear cache cho tất cả các service thuộc họ Research.Services.Security
    /// </summary>
    public interface ISecurityCacheWriter
    {
        /// <summary>
        /// lấy về những CustomerRole được phép tương tác sửa đổi đối tượng entity, static cache
        /// </summary>
        IList<int> GetCustomerRoleIdsWithAccess(int entityId, string entityName, Func<IList<int>> acquire);

        /// <summary>
        /// Cho biết có mối quan hệ map giữa permissionRecordSystemName - customerRoleId hay ko, static cache
        /// </summary>
        bool PermissionRecordMapCustomerRole(string permissionRecordSystemName, int customerRoleId, Func<bool> acquire);
    }
}
