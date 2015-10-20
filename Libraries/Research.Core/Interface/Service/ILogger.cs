using System;
using System.Collections.Generic;
using Research.Core;
using Research.Core.Domain.Logging;
using Research.Core.Domain.Customers;

namespace Research.Core.Interface.Service
{
    public partial interface ILogger
    {
        /// <summary>
        /// Kiểm tra xem 1 log level có được bật hay ko
        /// </summary>
        bool IsEnabled(LogLevel level);

        void Delete(Log entity);

        /// <summary>
        /// Clears a log
        /// </summary>
        void ClearLog();

        /// <summary>
        /// Gets all log items
        /// </summary>
        /// <param name="fromUtc">Log item creation from; null to load all records</param>
        /// <param name="toUtc">Log item creation to; null to load all records</param>
        /// <param name="message">Message</param>
        /// <param name="logLevel">Log level; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Log item collection</returns>
        IPagedList<Log> GetAllLogs(DateTime? fromUtc, DateTime? toUtc,
            string message, LogLevel? logLevel, int pageIndex, int pageSize);

        Log GetById(int id);

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        IList<Log> GetLogByIds(int[] logIds);

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <returns>A log item</returns>
        Log InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null);
    }
}
