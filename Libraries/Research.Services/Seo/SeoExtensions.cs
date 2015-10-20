using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Research.Core;
using Research.Core.Domain.Catalog;
using Research.Core.Domain.Seo;
using Research.Core.Infrastructure;
using Research.Services.Localization;
using Research.Core.Interface.Service;

namespace Research.Services.Seo
{
    public static class SeoExtensions
    {
        /// <summary>
        /// Từ bộ 3 giá trị đọc ra slug key tương ứng, tuy nhiên, có 1 khác biệt so với IUrlRecordService.GetActiveSlugCached()
        /// là hàm này cho phép chúng ta có thể lấy về slug mặc định với languageId=0 trong trường hợp languageId<>0 và ko tìm thấy
        /// urlRecord phù hợp
        /// 
        /// Chúng ta cũng có thể chỉ định rằng chỉ load urlRecord cho ngôn ngữ khác khi hệ thống có ít nhất 2 ngôn ngữ đang hoạt động, ngược lại
        /// thì cứ load theo languageid=0 cho nó lành
        /// </summary>
        /// /// <param name="entityId">Entity identifier</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if language specified one is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <returns>Search engine  name (slug)</returns>
        public static string GetSeName(int entityId, string entityName, int languageId, IUrlRecordService urlRecordService = null,
            bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true)
        {
            string result = string.Empty;
            var engine = EngineContext.Current;
            if(urlRecordService == null)
                urlRecordService = engine.Resolve<IUrlRecordService>();
            if (languageId > 0)
            {
                // giải quyết ràng buộc của ensureTwoPublishedLanguages
                bool loadLocalizedValue = true;
                if (ensureTwoPublishedLanguages)
                {
                    var languageService = engine.Resolve<ILanguageService>();
                    loadLocalizedValue = languageService.GetAllLanguages().Count >= 2;
                }
                // như vậy trong trường hợp chỉ có 1 ngôn ngữ active thì cho dù có định nghĩa seo url riêng cho ngôn ngữ đó, hệ thống
                // vẫn sẽ chọn dùng seo url mặc định với laguageid = 0
                if (loadLocalizedValue)
                    result = urlRecordService.GetActiveSlugCached(entityId, entityName, languageId);
            }
            if (string.IsNullOrEmpty(result) && returnDefaultValue)
                result = urlRecordService.GetActiveSlugCached(entityId, entityName, 0);
            return result;
        }
    }
}
