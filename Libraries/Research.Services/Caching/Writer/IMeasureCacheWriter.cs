using Research.Core.Domain.Directory;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    /// <summary>
    /// Cache cho cả MeasureDimension và MeasureWeight
    /// </summary>
    public interface IMeasureCacheWriter
    {
        #region MeasureDimension

        /// <summary>
        /// Lấy tất cả MeasureDimension, cache static
        /// </summary>
        IList<MeasureDimension> GetAllMeasureDimensionsCached(Func<IList<MeasureDimension>> acquire);

        /// <summary>
        /// Lấy tất cả MeasureDimension, cache per request
        /// </summary>
        IList<MeasureDimension> GetAllMeasureDimensions(bool getFromStaticCache, Func<IList<MeasureDimension>> acquire);

        /// <summary>
        /// Cache per request
        /// </summary>
        MeasureDimension GetMeasureDimensionById(int id, bool getFromStaticCache, Func<MeasureDimension> acquire);

        /// <summary>
        /// Cache per request
        /// </summary>
        MeasureDimension GetMeasureDimensionBySystemKeyword(string systemKeyword, bool getFromStaticCache,
            Func<MeasureDimension> acquire);

        #endregion

        #region MeasureWeight

        /// <summary>
        /// Lấy tất cả MeasureWeight, cache static
        /// </summary>
        IList<MeasureWeight> GetAllMeasureWeightsCached(Func<IList<MeasureWeight>> acquire);

        /// <summary>
        /// Lấy tất cả MeasureWeight, cache per request
        /// </summary>
        IList<MeasureWeight> GetAllMeasureWeights(bool getFromStaticCache, Func<IList<MeasureWeight>> acquire);

        /// <summary>
        /// Cache per request
        /// </summary>
        MeasureWeight GetMeasureWeightById(int id, bool getFromStaticCache, Func<MeasureWeight> acquire);

        /// <summary>
        /// Cache per request
        /// </summary>
        MeasureWeight GetMeasureWeightBySystemKeyword(string systemKeyword, bool getFromStaticCache,
            Func<MeasureWeight> acquire);

        #endregion
    }
}
