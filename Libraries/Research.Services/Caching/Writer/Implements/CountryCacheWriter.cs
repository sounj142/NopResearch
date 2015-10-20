using Research.Core.Domain.Directory;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class CountryCacheWriter : BaseCacheWriter, ICountryCacheWriter,
        ICacheConsumer<EntityInserted<Country>>,
        ICacheConsumer<EntityUpdated<Country>>,
        ICacheConsumer<EntityDeleted<Country>>,
        ICacheConsumer<EntityAllChange<Country>>
    {
        public IList<Country> GetAllFromStaticCache(Func<IList<Country>> acquire)
        {
            return GetFunc(CacheKey.COUNTRIES_ALL_KEY, acquire, true, false);
        }

        public Country GetCountryById(int countryId, bool getFromStaticCache, Func<Country> acquire)
        {
            string key = string.Format(CacheKey.COUNTRIES_BY_ID, countryId, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public IList<Country> GetAllCountries(bool autoOrder, bool showHidden, bool getFromStaticCache, Func<IList<Country>> acquire)
        {
            string key = string.Format(CacheKey.COUNTRIES_BY_ID, autoOrder, showHidden, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }





        public int Order
        {
            get { return 0; }
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.COUNTRIES_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.COUNTRIES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<Country> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<Country> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<Country> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<Country> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
