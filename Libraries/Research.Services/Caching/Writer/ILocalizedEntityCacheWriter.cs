using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface ILocalizedEntityCacheWriter
    {
        /// <summary>
        /// Trả về tất cả các LocalizedProperty được cache theo khóa là bộ 4 key EntityId_LanguageId_LocaleKeyGroup_LocaleKey,
        /// và giá trị là giá trị của LocaleValue
        /// </summary>
        IDictionary<string, string> GetAll(Func<IDictionary<string, string>> acquire);

        /// <summary>
        /// Lấy về giá trị chuỗi LocaleValue tương ứng với bộ 4 khóa
        /// </summary>
        string Get(int languageId, int entityId, string localeKeyGroup, string localeKey, Func<string> acquire);
    }
}
