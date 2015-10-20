using Research.Core.Domain.Directory;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Caching.Models;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class StateProvinceCacheWriter : BaseCacheWriter, IStateProvinceCacheWriter,
        ICacheConsumer<EntityInserted<StateProvince>>,
        ICacheConsumer<EntityUpdated<StateProvince>>,
        ICacheConsumer<EntityDeleted<StateProvince>>,
        ICacheConsumer<EntityAllChange<StateProvince>>
    {
        public StateProvinceCachePackage GetAllFromStaticCache(Func<StateProvinceCachePackage> acquire)
        {
            return GetFunc(CacheKey.STATEPROVINCES_ALL_KEY, acquire, true, false);
        }

        public StateProvince GetStateProvinceById(int id, bool getFromStaticCache, Func<StateProvince> acquire)
        {
            string key = string.Format(CacheKey.STATEPROVINCES_BY_ID, id, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public IList<StateProvince> GetStateProvincesByCountryId(int countryId, bool autoOrder, bool showHidden, 
            bool getFromStaticCache, Func<IList<StateProvince>> acquire)
        {
            string key = string.Format(CacheKey.STATEPROVINCES_BY_COUNTRYID_WITH_HIDDEN_KEY, countryId,
                autoOrder, showHidden, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }




        public int Order
        {
            get { return 0; }
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.STATEPROVINCES_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.STATEPROVINCES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<StateProvince> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<StateProvince> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<StateProvince> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<StateProvince> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
