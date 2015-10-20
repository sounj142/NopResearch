using Research.Core;
using Research.Core.Domain.Customers;
using Research.Core.Interface.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Research.Services.Helpers
{
    public partial class DateTimeHelper : IDateTimeHelper
    {
        #region Field, property, ctor

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISettingService _settingService;
        private readonly DateTimeSettings _dateTimeSettings;

        public DateTimeHelper(IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            ISettingService settingService,
            DateTimeSettings dateTimeSettings,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
            _dateTimeSettings = dateTimeSettings;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public virtual TimeZoneInfo FindTimeZoneById(string id)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }

        public virtual ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones();
        }

        public virtual DateTime ConvertToUserTime(DateTime dt)
        {
            return ConvertToUserTime(dt, dt.Kind);
        }

        public virtual DateTime ConvertToUserTime(DateTime dt, DateTimeKind sourceDateTimeKind)
        {
            dt = DateTime.SpecifyKind(dt, sourceDateTimeKind);
            return TimeZoneInfo.ConvertTime(dt, this.CurrentTimeZone);
        }

        public virtual DateTime ConvertToUserTime(DateTime dt, TimeZoneInfo sourceTimeZone)
        {
            return ConvertToUserTime(dt, sourceTimeZone, this.CurrentTimeZone);
        }

        public virtual DateTime ConvertToUserTime(DateTime dt, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(dt, sourceTimeZone, destinationTimeZone);
        }

        public virtual DateTime ConvertToUtcTime(DateTime dt)
        {
            return ConvertToUtcTime(dt, dt.Kind);
        }

        public virtual DateTime ConvertToUtcTime(DateTime dt, DateTimeKind sourceDateTimeKind)
        {
            dt = DateTime.SpecifyKind(dt, sourceDateTimeKind);
            return TimeZoneInfo.ConvertTimeToUtc(dt);
        }

        public virtual DateTime ConvertToUtcTime(DateTime dt, TimeZoneInfo sourceTimeZone)
        {
            if (sourceTimeZone.IsInvalidTime(dt)) return dt;
            return TimeZoneInfo.ConvertTimeToUtc(dt, sourceTimeZone);
        }

        public virtual TimeZoneInfo GetCustomerTimeZone(Customer customer)
        {
            TimeZoneInfo timeZoneInfo = null;
            if (_dateTimeSettings.AllowCustomersToSetTimeZone && customer != null)
            {
                // nếu có thì lấy time zone của customer. Thông tin time zone của customer được lưu trong generic attribute
                string timeZoneId = customer.GetAttribute<string>(SystemCustomerAttributeNames.TimeZoneId, _genericAttributeService);
                if(!string.IsNullOrEmpty(timeZoneId))
                    try
                    {
                        timeZoneInfo = FindTimeZoneById(timeZoneId);
                    }catch(Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
            }
            // nếu ko có, lấy time zone của store
            if (timeZoneInfo == null) timeZoneInfo = DefaultStoreTimeZone;
            return timeZoneInfo;
        }

        public virtual TimeZoneInfo DefaultStoreTimeZone
        {
            get
            {
                TimeZoneInfo timeZoneInfo = null;
                try
                {
                    // time zone của store được cấu hình trong bảng Settings
                    if(!string.IsNullOrEmpty(_dateTimeSettings.DefaultStoreTimeZoneId))
                        timeZoneInfo = FindTimeZoneById(_dateTimeSettings.DefaultStoreTimeZoneId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                // nếu ko có thì lấy time zone local của hệ thống đang chạy
                if (timeZoneInfo == null) timeZoneInfo = TimeZoneInfo.Local;
                return timeZoneInfo;
            }
            set
            {
                _dateTimeSettings.DefaultStoreTimeZoneId = (value != null ? value.Id : string.Empty);
                _settingService.SaveSetting(_dateTimeSettings, _storeContext.CurrentStore.Id); // khác ở đây
            }
        }

        public virtual TimeZoneInfo CurrentTimeZone
        { 
            get
            {
                return GetCustomerTimeZone(_workContext.CurrentCustomer);
            }
            set
            {
                if (!_dateTimeSettings.AllowCustomersToSetTimeZone) return;

                // khác ở đây: time zone được thiết lập riêng cho từng store riêng lẻ, ko như NOP thiết lập time zone chung
                // với storeId = 0
                string timeZoneId = value != null ? value.Id : string.Empty;
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.TimeZoneId,
                    timeZoneId, _storeContext.CurrentStore.Id);
            }
        }

        #endregion


        
    }
}
