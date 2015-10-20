using Research.Core.Caching;
using Research.Core.Domain.Localization;
using Research.Core.Events;
using Research.Services.Caching.Models;
using Research.Services.Events;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer.Implements
{
    /// <summary>
    /// Sẽ xử lý theo cách cache static toàn bộ bảng Language
    /// Ngoài ra sẽ cache riêng theo per request cache để tăng cường hiệu năng
    /// => lúc clear cần clear cả 2 cache để tăng độ chính xác
    /// </summary>
    public class LanguageCacheWriter : BaseCacheWriter, ILanguageCacheWriter,
        ICacheConsumer<EntityInserted<Language>>,
        ICacheConsumer<EntityUpdated<Language>>,
        ICacheConsumer<EntityDeleted<Language>>,
        ICacheConsumer<EntityAllChange<Language>>
    {
        /// <summary>
        /// Lấy về tất cả các ngôn ngữ. Cache static
        /// </summary>
        public IList<LanguageForCache> GetAll(Func<IList<LanguageForCache>> acquire)
        {
            return this.GetFunc(CacheKey.LANGUAGES_ALL_KEY, acquire, true, false);
        }
        /// <summary>
        /// Lấy về tất cả các ngôn ngữ theo Cache per request
        /// </summary>
        public IList<Language> GetAll(bool showHidden, int storeId, bool getFromStaticCache, Func<IList<Language>> acquire)
        {
            string key = string.Format(CacheKey.LANGUAGES_ALL_HASHIDDEN_KEY, showHidden, storeId, getFromStaticCache);
            return GetFunc(key, acquire, false, true);
        }

        public Language Get(int languageId, bool getFromStaticCache, Func<Language> acquire)
        {
            string key = string.Format(CacheKey.LANGUAGES_BY_ID_KEY, languageId, getFromStaticCache);
            return this.GetFunc(key, acquire, false, true);
        }

        public int Order
        {
            get { return 0; }
        }

        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            staticCachePrefixes.Add(CacheKey.LANGUAGES_PATTERN_KEY);
            perRequestCachePrefixes.Add(CacheKey.LANGUAGES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<Language> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<Language> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<Language> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<Language> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
