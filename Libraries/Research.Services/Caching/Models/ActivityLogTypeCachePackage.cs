using Research.Core.Domain.Logging;
using System.Collections.Generic;

namespace Research.Services.Caching.Models
{
    public class ActivityLogTypeCachePackage
    {
        public IList<ActivityLogType> ActivityLogTypeList { get; private set; }
        public IDictionary<string, ActivityLogType> ActivityLogTypeDict { get; private set; }

        public ActivityLogTypeCachePackage(IList<ActivityLogType> activityLogTypeList,
            IDictionary<string, ActivityLogType> activityLogTypeDict)
        {
            this.ActivityLogTypeList = activityLogTypeList;
            this.ActivityLogTypeDict = activityLogTypeDict;
        }
    }
}
