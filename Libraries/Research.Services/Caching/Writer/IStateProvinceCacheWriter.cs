using Research.Core.Domain.Directory;
using Research.Services.Caching.Models;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface IStateProvinceCacheWriter
    {
        /// <summary>
        /// get all, static cache
        /// </summary>
        StateProvinceCachePackage GetAllFromStaticCache(Func<StateProvinceCachePackage> acquire);

        /// <summary>
        /// get by id, per request cache
        /// </summary>
        StateProvince GetStateProvinceById(int id, bool getFromStaticCache, Func<StateProvince> acquire);

        /// <summary>
        /// Cache per request
        /// </summary>
        IList<StateProvince> GetStateProvincesByCountryId(int countryId, bool autoOrder, bool showHidden, 
            bool getFromStaticCache, Func<IList<StateProvince>> acquire);
    }
}
