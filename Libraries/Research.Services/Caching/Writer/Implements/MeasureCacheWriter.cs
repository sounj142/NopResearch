using Research.Core.Domain.Directory;
using Research.Core.Events;
using Research.Core.Caching;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    public class MeasureCacheWriter : BaseCacheWriter, IMeasureCacheWriter,
        ICacheConsumer<EntityInserted<MeasureDimension>>,
        ICacheConsumer<EntityUpdated<MeasureDimension>>,
        ICacheConsumer<EntityDeleted<MeasureDimension>>,
        ICacheConsumer<EntityAllChange<MeasureDimension>>,
        ICacheConsumer<EntityInserted<MeasureWeight>>,
        ICacheConsumer<EntityUpdated<MeasureWeight>>,
        ICacheConsumer<EntityDeleted<MeasureWeight>>,
        ICacheConsumer<EntityAllChange<MeasureWeight>>
    {

        #region MeasureDimension

        public IList<MeasureDimension> GetAllMeasureDimensionsCached(Func<IList<MeasureDimension>> acquire)
        {
            return GetFunc(CacheKey.MEASUREDIMENSIONS_ALL_KEY, acquire, true, false);
        }

        public IList<MeasureDimension> GetAllMeasureDimensions(bool getFromStaticCache, Func<IList<MeasureDimension>> acquire)
        {
            string key = string.Format(CacheKey.MEASUREDIMENSIONS_ALL_KEY_PERREQUEST, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public MeasureDimension GetMeasureDimensionById(int id, bool getFromStaticCache, Func<MeasureDimension> acquire)
        {
            string key = string.Format(CacheKey.MEASUREDIMENSIONS_BY_ID_KEY, id, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public MeasureDimension GetMeasureDimensionBySystemKeyword(string systemKeyword, bool getFromStaticCache,
            Func<MeasureDimension> acquire)
        {
            string key = string.Format(CacheKey.MEASUREDIMENSIONS_BY_SYSTEM_KEYWORD, systemKeyword, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }


        private void AddCacheToClearMeasureDimension(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.MEASUREDIMENSIONS_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.MEASUREDIMENSIONS_PATTERN_KEY);
        }

        #endregion

        #region MeasureWeight

        public IList<MeasureWeight> GetAllMeasureWeightsCached(Func<IList<MeasureWeight>> acquire)
        {
            return GetFunc(CacheKey.MEASUREWEIGHTS_ALL_KEY, acquire, true, false);
        }

        public IList<MeasureWeight> GetAllMeasureWeights(bool getFromStaticCache, Func<IList<MeasureWeight>> acquire)
        {
            string key = string.Format(CacheKey.MEASUREWEIGHTS_ALL_KEY_PERREQUEST, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public MeasureWeight GetMeasureWeightById(int id, bool getFromStaticCache, Func<MeasureWeight> acquire)
        {
            string key = string.Format(CacheKey.MEASUREWEIGHTS_BY_ID_KEY, id, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public MeasureWeight GetMeasureWeightBySystemKeyword(string systemKeyword, bool getFromStaticCache,
            Func<MeasureWeight> acquire)
        {
            string key = string.Format(CacheKey.MEASUREWEIGHTS_BY_SYSTEM_KEYWORD, systemKeyword, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        private void AddCacheToClearMeasureWeight(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.MEASUREWEIGHTS_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.MEASUREWEIGHTS_PATTERN_KEY);
        }

        #endregion

        


        

        

        public int Order
        {
            get { return 0; }
        }

        public void HandleEvent(EntityInserted<MeasureDimension> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearMeasureDimension(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<MeasureDimension> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearMeasureDimension(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<MeasureDimension> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearMeasureDimension(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<MeasureDimension> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearMeasureDimension(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityInserted<MeasureWeight> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearMeasureWeight(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<MeasureWeight> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearMeasureWeight(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<MeasureWeight> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearMeasureWeight(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<MeasureWeight> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClearMeasureWeight(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
