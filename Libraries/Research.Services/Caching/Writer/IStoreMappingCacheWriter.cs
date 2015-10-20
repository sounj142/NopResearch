using Research.Core.Domain.Stores;
using Research.Services.Caching.Models;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface IStoreMappingCacheWriter
    {
        int[] GetStoresIdsWithAccess(int entityId, string entityName, Func<int[]> acquire);
    }
}
