using Research.Core.Domain.Logging;
using Research.Core.Events;
using Research.Services.Caching.Models;
using Research.Services.Events;
using Research.Core.Caching;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class ActivityLogTypeCacheWriter : BaseCacheWriter, IActivityLogTypeCacheWriter,
        ICacheConsumer<EntityInserted<ActivityLogType>>,
        ICacheConsumer<EntityUpdated<ActivityLogType>>,
        ICacheConsumer<EntityDeleted<ActivityLogType>>,
        ICacheConsumer<EntityAllChange<ActivityLogType>>
    {
        public ActivityLogTypeCachePackage GetAll(Func<ActivityLogTypeCachePackage> acquire)
        {
            return GetFunc(CacheKey.ACTIVITYTYPE_ALL_KEY, acquire, true, false);
        }


        public int Order
        {
            get { return 0; }
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.ACTIVITYTYPE_PATTERN_KEY);
            //perRequestCachePrefixes.Add(CacheKey.LANGUAGES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<ActivityLogType> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<ActivityLogType> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<ActivityLogType> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<ActivityLogType> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
