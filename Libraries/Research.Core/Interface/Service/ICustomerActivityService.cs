using Research.Core.Domain.Customers;
using Research.Core.Domain.Logging;
using System;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// service ghi log các hành vi của người dùng trong hệ thống, chẳng hạn chỉnh sửa 1 sản phẩm, thêm 1 sản phẩm, xóa 1 category, ..v..v..
    /// . Hệ thống ghi log của Nop chỉ ghi log ở múc chung chung, kiểu như là ai đó làm loại công việc gì, nhưng ko ghi cụ thể là làm cái gì
    /// , ko cho biết data nào bị tác động và bị tác động ra sao.
    /// 
    /// Dùng cache static để cache danh sách các kiểu log ActivityLogType
    /// </summary>
    public partial interface ICustomerActivityService
    {
        void Insert(ActivityLogType entity);

        void Delete(ActivityLogType entity);

        void Update(ActivityLogType entity);

        /// <summary>
        /// ko cache per request
        /// </summary>
        IList<ActivityLogType> GetAllActivityTypes(bool getFromCache = true);

        /// <summary>
        /// ko cache per request
        /// </summary>
        ActivityLogType GetActivityTypeById(int activityLogTypeId, bool getFromCache = true);

        /// <summary>
        /// Inserts an activity log item.
        /// Sẽ ghi 1 log hoạt động vào danh sách log của nguwofi dùng hiện hành
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams);

        /// <summary>
        /// Inserts an activity log item.
        /// Ghi 1 log hoạt động vào danh sách log của người dùng customer
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="customer">The customer</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        ActivityLog InsertActivity(string systemKeyword,
            string comment, Customer customer, params object[] commentParams);

        void Delete(ActivityLog entity);

        /// <summary>
        /// Gets all activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; null to load all customers</param>
        /// <param name="createdOnTo">Log item creation to; null to load all customers</param>
        /// <param name="customerId">Customer identifier; null to load all customers</param>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Activity log collection</returns>
        IPagedList<ActivityLog> GetAllActivities(DateTime? createdOnFrom,
            DateTime? createdOnTo, int? customerId,
            int? activityLogTypeId, int pageIndex, int pageSize);

        ActivityLog GetActivityById(int activityLogId);

        /// <summary>
        /// Clears activity log
        /// </summary>
        void ClearAllActivities();
    }
}
