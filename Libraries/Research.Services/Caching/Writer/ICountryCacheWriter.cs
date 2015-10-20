using Research.Core.Domain.Directory;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface ICountryCacheWriter
    {
        /// <summary>
        /// static cache
        /// </summary>
        IList<Country> GetAllFromStaticCache(Func<IList<Country>> acquire);

        /// <summary>
        /// per request cache
        /// </summary>
        Country GetCountryById(int countryId, bool getFromStaticCache, Func<Country> acquire);

        /// <summary>
        /// lấy ra danh sách các country, cache per request
        /// </summary>
        IList<Country> GetAllCountries(bool autoOrder, bool showHidden, bool getFromStaticCache, Func<IList<Country>> acquire);
    }
}
