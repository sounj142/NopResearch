using Research.Core;
using Research.Core.Domain.Common;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Logging;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Caching.Models;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Services.Logging
{
    public partial class CustomerActivityService : BaseService<ActivityLogType>, ICustomerActivityService
    {
        #region Field, ctor, property

        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IWorkContext _workContext;
        private readonly CommonSettings _commonSettings;
        private readonly IActivityLogTypeCacheWriter _cacheWriter;

        public CustomerActivityService(IRepository<ActivityLogType> repository,
            IEventPublisher eventPublisher,
            IActivityLogRepository activityLogRepository,
            IWorkContext workContext,
            CommonSettings commonSettings,
            IActivityLogTypeCacheWriter cacheWriter)
            : base(repository, eventPublisher)
        {
            _activityLogRepository = activityLogRepository;
            _workContext = workContext;
            _commonSettings = commonSettings;
            _cacheWriter = cacheWriter;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Lấy về tất cả các ActivityLogType, static cache. Để đảm bảo tính toàn vẹn dữ liệu, dữ liệu lấy ra từ hàm này 
        /// cần được clone lại trước khi cung cấp ra cho bên ngoài sử dụng
        /// </summary>
        /// <returns></returns>
        protected virtual ActivityLogTypeCachePackage GetAllActivityTypesCached()
        {
            return _cacheWriter.GetAll(() => {
                var list = _repository.TableNoTracking.OrderBy(p => p.Name).ToList();
                var dict = new Dictionary<string, ActivityLogType>();
                foreach (var item in list)
                    if (!string.IsNullOrEmpty(item.SystemKeyword))
                        dict[item.SystemKeyword] = item;

                return new ActivityLogTypeCachePackage(list, dict);
            });
        }

        #endregion

        #region Methods

        #region ActivityLogType

        public virtual IList<ActivityLogType> GetAllActivityTypes(bool getFromCache = true)
        {
            return getFromCache
                ? GetAllActivityTypesCached().ActivityLogTypeList.Select(p => p.MakeClone()).ToList()
                : _repository.Table.OrderBy(p => p.Name).ToList();
        }

        public virtual ActivityLogType GetActivityTypeById(int activityLogTypeId, bool getFromCache = true)
        {
            if (activityLogTypeId <= 0) return null;
            return getFromCache // thao tác get từ static cache này có thể hơi chậm nếu lượng dữ liệu lớn, tuy nhiên ở đây chỉ có khoảng 60 dòng dữ liệu
                ? ActivityLogType.MakeClone(GetAllActivityTypesCached().ActivityLogTypeList
                        .FirstOrDefault(p => p.Id == activityLogTypeId))
                : _repository.GetById(activityLogTypeId);
        }

        #endregion

        #region ActivityLog

        public virtual ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams)
        {
            return InsertActivity(systemKeyword, comment, _workContext.CurrentCustomer, commentParams);
        }

        public virtual ActivityLog InsertActivity(string systemKeyword, string comment, Customer customer, 
            params object[] commentParams)
        {
            if (customer == null) return null;
            if (string.IsNullOrEmpty(systemKeyword)) return null;

            var dict = GetAllActivityTypesCached().ActivityLogTypeDict;
            ActivityLogType activityLogType;
            if (!dict.TryGetValue(systemKeyword, out activityLogType) || 
                activityLogType == null || !activityLogType.Enabled) return null;
            
            comment = comment ?? string.Empty;
            if (commentParams != null && commentParams.Length > 0)
                comment = string.Format(comment, commentParams);
            comment = CommonHelper.EnsureMaximumLength(comment, _commonSettings.ActivityLogCommentMaxLength);

            var activity = new ActivityLog
            {
                ActivityLogTypeId = activityLogType.Id,
                Comment = comment,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customer.Id
            };
            _activityLogRepository.Insert(activity);
            _unitOfWork.SaveChanges();
            return activity;
        }

        public virtual void Delete(ActivityLog entity)
        {
            // khi thêm xóa sửa trên các mục ghi log thì sẽ ko có bất kỳ 1 sự kiện nào được ném ra hay 1 thao tác clear cache đc thực
            // hiện vì log được coi là cần đc tách rời ra khỏi hệ thống, ko liên quan nhiều đến cái khác
            if (entity == null) throw new ArgumentNullException("entity");
            _activityLogRepository.Delete(entity);
            _unitOfWork.SaveChanges();
        }

        public virtual IPagedList<ActivityLog> GetAllActivities(DateTime? createdOnFrom, DateTime? createdOnTo, 
            int? customerId, int? activityLogTypeId, int pageIndex, int pageSize)
        {
            var query = _activityLogRepository.Table;
            if (createdOnFrom.HasValue)
                query = query.Where(p => p.CreatedOnUtc >= createdOnFrom.Value);
            if (createdOnTo.HasValue)
                query = query.Where(p => p.CreatedOnUtc <= createdOnTo.Value);
            if (customerId.HasValue)
                query = query.Where(p => p.CustomerId == customerId.Value);
            if (activityLogTypeId.HasValue)
                query = query.Where(p => p.ActivityLogTypeId == activityLogTypeId.Value);

            query = query.OrderByDescending(p => p.CreatedOnUtc);
            return new PagedList<ActivityLog>(query, pageIndex, pageSize);
        }

        public virtual ActivityLog GetActivityById(int activityLogId)
        {
            if (activityLogId <= 0) return null;
            return _activityLogRepository.GetById(activityLogId);
        }

        public virtual void ClearAllActivities()
        {
            _activityLogRepository.ClearAllActivities(_commonSettings.UseStoredProceduresIfSupported);
        }

        #endregion

        #endregion

        

        
    }
}
