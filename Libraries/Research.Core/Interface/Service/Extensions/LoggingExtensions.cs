using System;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Logging;
using System.Threading;

namespace Research.Core.Interface.Service
{
    public static class LoggingExtensions
    {
        private static void FilteredLog(ILogger logger, LogLevel level, string message,
            Exception exception = null, Customer customer = null, bool stopException = true)
        {
            // không log thông điệp khi ngoại lệ là ThreadAbortException
            if (exception is ThreadAbortException) return;
            try
            {
                if (logger.IsEnabled(level)) // nếu cho phép ghi log với kiểu level
                {
                    string fullMessage = exception == null ? string.Empty : exception.ToString();
                    logger.InsertLog(level, message, fullMessage, customer);
                }
            }
            catch (Exception)
            {
                if (!stopException) throw;
            }
        }

        public static void Debug(this ILogger logger, string message, Exception ex = null,
            Customer customer = null, bool stopException = true)
        {
            FilteredLog(logger, LogLevel.Debug, message, ex, customer);
        }

        public static void Information(this ILogger logger, string message, Exception ex = null,
            Customer customer = null, bool stopException = true)
        {
            FilteredLog(logger, LogLevel.Information, message, ex, customer);
        }

        public static void Warning(this ILogger logger, string message, Exception ex = null,
            Customer customer = null, bool stopException = true)
        {
            FilteredLog(logger, LogLevel.Warning, message, ex, customer);
        }

        /// <summary>
        /// Hàm thuận tiện để ghi log với loglevel tương ứng mà ko cần kiểm tra IsEnabled và gọi hàm InsertLog
        /// </summary>
        public static void Error(this ILogger logger, string message, Exception ex = null,
            Customer customer = null, bool stopException = true)
        {
            FilteredLog(logger, LogLevel.Error, message, ex, customer);
        }

        public static void Fatal(this ILogger logger, string message, Exception ex = null,
            Customer customer = null, bool stopException = true)
        {
            FilteredLog(logger, LogLevel.Fatal, message, ex, customer);
        }
    }
}
