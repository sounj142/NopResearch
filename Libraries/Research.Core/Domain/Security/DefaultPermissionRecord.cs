using System.Collections.Generic;

namespace Research.Core.Domain.Security
{
    /// <summary>
    /// Represents a default permission record
    /// Miêu tả nhóm quyền con mặc định ban đầu của từng Role, VD như Role Anfa gồm các quyền InventoryManager, NewsManager,..v.v.v.
    /// </summary>
    public class DefaultPermissionRecord
    {
        public DefaultPermissionRecord()
        {
            this.PermissionRecords = new List<PermissionRecord>();
        }

        /// <summary>
        /// Gets or sets the customer role system name
        /// </summary>
        public string CustomerRoleSystemName { get; set; }

        /// <summary>
        /// Gets or sets the permissions
        /// </summary>
        public IEnumerable<PermissionRecord> PermissionRecords { get; set; }
    }
}
