using System;
using System.Collections.Generic;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Events;
using Research.Core.Domain.Security;

namespace Research.Services.Caching.Writer.Implements
{
    public class SecurityCacheWriter : BaseCacheWriter, ISecurityCacheWriter,
        ICacheConsumer<EntityInserted<AclRecord>>,
        ICacheConsumer<EntityUpdated<AclRecord>>,
        ICacheConsumer<EntityDeleted<AclRecord>>,
        ICacheConsumer<EntityAllChange<AclRecord>>,
        ICacheConsumer<EntityInserted<PermissionRecord>>,
        ICacheConsumer<EntityUpdated<PermissionRecord>>,
        ICacheConsumer<EntityDeleted<PermissionRecord>>,
        ICacheConsumer<EntityAllChange<PermissionRecord>>
    {
        #region Cache methods

        public IList<int> GetCustomerRoleIdsWithAccess(int entityId, string entityName, Func<IList<int>> acquire)
        {
            string key = string.Format(CacheKey.ACLRECORD_BY_ENTITYID_NAME_KEY, entityId, entityName);
            return GetFunc(key, acquire, true, false);
        }

        public bool PermissionRecordMapCustomerRole(string permissionRecordSystemName, int customerRoleId, Func<bool> acquire)
        {
            string key = string.Format(CacheKey.PERMISSIONS_ALLOWED_KEY, customerRoleId, permissionRecordSystemName);
            return GetFunc(key, acquire, true, false);
        }

        #endregion

        #region Event methods

        private void AddCacheToClearAclRecord(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.ACLRECORD_PATTERN_KEY);
        }

        private void AddCacheToClearPermissionRecord(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.PERMISSIONS_PATTERN_KEY);
        }

        public int Order
        {
            get {  return 0; }
        }

        public void HandleEvent(EntityInserted<AclRecord> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearAclRecord(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<AclRecord> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearAclRecord(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<AclRecord> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearAclRecord(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<AclRecord> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearAclRecord(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityInserted<PermissionRecord> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearPermissionRecord(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<PermissionRecord> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearPermissionRecord(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<PermissionRecord> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearPermissionRecord(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<PermissionRecord> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearPermissionRecord(staticCachePrefixes, perRequestCachePrefixes);
        }

        #endregion

    }
}
