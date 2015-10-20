using Research.Core.Domain.Localization;
using Research.Services.Caching.Models;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface ILanguageCacheWriter
    {
        /// <summary>
        /// Lấy về tất cả các ngôn ngữ. Cache static
        /// </summary>
        IList<LanguageForCache> GetAll(Func<IList<LanguageForCache>> acquire);

        /// <summary>
        /// Lấy về tất cả các ngôn ngữ theo Cache per request
        /// </summary>
        IList<Language> GetAll(bool showHidden, int storeId, bool getFromStaticCache, Func<IList<Language>> acquire);

        Language Get(int languageId, bool getFromStaticCache, Func<Language> acquire);
    }
}