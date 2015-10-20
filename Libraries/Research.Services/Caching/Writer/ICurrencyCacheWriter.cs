using Research.Core.Domain.Directory;
using Research.Services.Caching.Models;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface ICurrencyCacheWriter
    {
        /// <summary>
        /// Hàm cache static tất cả các Currency trong hệ thống
        /// </summary>
        IList<CurrencyForCache> GetAll(Func<IList<CurrencyForCache>> acquire);

        /// <summary>
        /// Hàm lấy Currency theo id, per request cache
        /// </summary>
        Currency GetById(int id, bool getFromStaticCache, Func<Currency> acquire);

        /// <summary>
        /// Hàm lấy Currency theo mã tiền tệ, per request cache
        /// </summary>
        Currency GetByCurrencyCode(string currencyCode, bool getFromStaticCache, Func<Currency> acquire);

        /// <summary>
        /// Hàm lấy curency theo các điều kiện về cờ ẩn hiện và store mapping, cache per request
        /// </summary>
        IList<Currency> GetAll(bool showHidden, int storeId, bool getFromStaticCache, Func<IList<Currency>> acquire);
    }
}
