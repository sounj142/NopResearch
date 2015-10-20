using System;
using System.Collections.Generic;
using Research.Core;
using Research.Core.Domain.Logging;
using Research.Core.Interface.Service;
using System.Linq;
using System.Linq.Expressions;
using Research.Core.Domain.Customers;

namespace Research.Services.Logging
{
    public partial class NullLogger : ILogger
    {

        public bool IsEnabled(LogLevel level)
        {
            return false;
        }

        public void Delete(Log entity)
        {
        }

        public void ClearLog()
        {
        }

        public IPagedList<Log> GetAllLogs(DateTime? fromUtc, DateTime? toUtc, string message, LogLevel? logLevel, int pageIndex, int pageSize)
        {
            return new PagedList<Log>(new List<Log>(), pageIndex, pageSize);
        }

        public Log GetById(int id)
        {
            return null;
        }

        public IList<Log> GetLogByIds(int[] logIds)
        {
            return new List<Log>();
        }

        public Log InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            return null;
        }
    }
}
