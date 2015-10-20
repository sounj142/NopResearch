using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface ILocalizationCacheWriter
    {
        /// <summary>
        /// Hàm cho phép cache tất cả các resource thuộc ngôn ngữ languageId vào static cache
        /// </summary>
        IDictionary<string, KeyValuePair<int, string>> GetAllByLaguageId(int languageId,
            Func<IDictionary<string, KeyValuePair<int, string>>> acquire);

        string GetByResourceKeyAndLanguageId(string resourceKey, int languageId, Func<string> acquire);
    }
}
