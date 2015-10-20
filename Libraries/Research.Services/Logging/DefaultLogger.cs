using System;
using System.Collections.Generic;
using Research.Core;
using Research.Core.Domain.Logging;
using Research.Core.Interface.Service;
using System.Linq;
using Research.Core.Interface.Data;
using Research.Core.Domain.Common;
using Research.Core.Domain.Customers;

namespace Research.Services.Logging
{
    /// <summary>
    /// Cài đặt của đối tượng log service mặc định, cho phép ghi log vào bảng tương ứng trong database
    /// Hàm ghi log sẽ ko publish event
    /// </summary>
    public partial class DefaultLogger : BaseService<Log>, ILogger
    {
        #region Fields, Properties and ctors

        private readonly IWebHelper _webHelper;
        private readonly CommonSettings _commonSettings;

        protected ILoggerRepository Repository
        {
            get { return (ILoggerRepository)_repository; }
        }

        public DefaultLogger(ILoggerRepository repository,
            IWebHelper webHelper,
            CommonSettings commonSettings)
            : base(repository, null) // truyền vào null để ko ném ra event
        {
            _webHelper = webHelper;
            _commonSettings = commonSettings;
        }

        #endregion 

        #region Utitilities

        /// <summary>
        /// Lấy ra 1 giá trị bool chỉ ra rằng message này có cần được log hay ko
        /// </summary>
        protected virtual bool IgnoreLog(string message)
        {
            if (_commonSettings.IgnoreLogWordlist.Count == 0) return false;
            if (string.IsNullOrWhiteSpace(message)) return false; 

            return _commonSettings.IgnoreLogWordlist
                .Any(x => message.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether a log level is enabled
        /// Mặn định là ko ghi log với loại level là Debug
        /// </summary>
        public virtual bool IsEnabled(LogLevel level)
        {
            return level != LogLevel.Debug;
        }

        public virtual void ClearLog()
        {
            Repository.ClearLog(_commonSettings.UseStoredProceduresIfSupported);
        }

        public virtual IPagedList<Log> GetAllLogs(DateTime? fromUtc, DateTime? toUtc, string message,
            LogLevel? logLevel, int pageIndex, int pageSize)
        {
            var query = _repository.Table;
            if (fromUtc.HasValue)
                query = query.Where(p => p.CreatedOnUtc >= fromUtc.Value);
            if (toUtc.HasValue)
                query = query.Where(p => p.CreatedOnUtc <= toUtc.Value);
            if (logLevel.HasValue)
                query = query.Where(p => p.LogLevelId == (int)logLevel.Value);
            if (!string.IsNullOrEmpty(message))
                query = query.Where(p => p.ShortMessage.Contains(message) || p.FullMessage.Contains(message));
            query = query.OrderByDescending(p => p.CreatedOnUtc);

            var log = new PagedList<Log>(query, pageIndex, pageSize);
            return log;
        }

        /// <summary>
        /// Tìm danh sách log theo id, kết quả trả về theo đúng thứ tự id của mẳng đầu vào
        /// </summary>
        public virtual IList<Log> GetLogByIds(int[] logIds)
        {
            if (logIds == null || logIds.Length == 0) return new List<Log>();

            var listLogs = _repository.Table.Where(p => logIds.Contains(p.Id)).ToList();
            var result = new List<Log>(listLogs.Count);
            foreach (int id in logIds)
            {
                var log = listLogs.Find(p => p.Id == id);
                if (log != null) result.Add(log);
            }
            return result;
        }

        /// <summary>
        /// Hàm insert 1 dòng thông báo log vào bảng log trong database
        /// </summary>
        public virtual Log InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage)) return null; // nếu mẫu log thuộc danh sách bị bỏ qua thì sẽ bỏ
            // việc ghi log và trả về null

            var log = new Log
            {
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customer != null ? (int?)customer.Id : null,
                FullMessage = fullMessage,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                LogLevel = logLevel,
                PageUrl = _webHelper.GetThisPageUrl(true),
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                ShortMessage = shortMessage
            };
            base.Insert(log);
            return log;
        }

        #endregion
    }
}
