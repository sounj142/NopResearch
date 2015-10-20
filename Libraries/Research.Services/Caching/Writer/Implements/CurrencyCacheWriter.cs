using Research.Core.Events;
using Research.Core.Domain.Directory;
using Research.Core.Caching;
using Research.Services.Events;
using System.Collections.Generic;
using System;
using Research.Services.Caching.Models;

namespace Research.Services.Caching.Writer.Implements
{
    /// <summary>
    /// Ko giống như Nop chỉ cache per request, chúng ta sẽ cache static để cache tất cả các lọai tiền tệ, và dùng cache per request
    /// để cache lại theo id/cache danh sách theo tình trạng hidden
    /// </summary>
    public class CurrencyCacheWriter : BaseCacheWriter, ICurrencyCacheWriter,
        ICacheConsumer<EntityInserted<Currency>>,
        ICacheConsumer<EntityUpdated<Currency>>,
        ICacheConsumer<EntityDeleted<Currency>>,
        ICacheConsumer<EntityAllChange<Currency>>
    {
        public IList<CurrencyForCache> GetAll(Func<IList<CurrencyForCache>> acquire)
        {
            return GetFunc(CacheKey.CURRENCIES_ALL_KEY, acquire, true, false);
        }

        public Currency GetById(int id, bool getFromStaticCache, Func<Currency> acquire)
        {
            string key = string.Format(CacheKey.CURRENCIES_BY_ID_KEY, id, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public Currency GetByCurrencyCode(string currencyCode, bool getFromStaticCache, Func<Currency> acquire)
        {
            string key = string.Format(CacheKey.CURRENCIES_BY_CODE, currencyCode, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public IList<Currency> GetAll(bool showHidden, int storeId, bool getFromStaticCache, Func<IList<Currency>> acquire)
        {
            string key = string.Format(CacheKey.CURRENCIES_ALL_KEY_WITH_HIDDEN, showHidden, storeId, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.CURRENCIES_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.CURRENCIES_PATTERN_KEY);
        }

        public int Order
        {
            get { return 0; }
        }

        public void HandleEvent(EntityInserted<Currency> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<Currency> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<Currency> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<Currency> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
