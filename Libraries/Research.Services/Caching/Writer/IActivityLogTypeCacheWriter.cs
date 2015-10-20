using Research.Services.Caching.Models;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface IActivityLogTypeCacheWriter
    {
        /// <summary>
        /// Lấy tất cả các ActivityLogType, cache static
        /// </summary>
        ActivityLogTypeCachePackage GetAll(Func<ActivityLogTypeCachePackage> acquire);
    }
}
