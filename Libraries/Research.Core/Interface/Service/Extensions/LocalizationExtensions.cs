using System;
using System.Linq.Expressions;
using System.Reflection;
using Research.Core;
using Research.Core.Configuration;
using Research.Core.Domain.Localization;
using Research.Core.Domain.Security;
using Research.Core.Infrastructure;
using Research.Core.Plugins;

namespace Research.Core.Interface.Service
{
    public static class LocalizationExtensions
    {
        public static string GetLocalized<T>(this T entity, Expression<Func<T, string>> keySelector,
            bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true)
            where T : BaseEntity, ILocalizedEntity
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            return GetLocalized<T, string>(entity, keySelector, workContext.WorkingLanguage.Id, 
                returnDefaultValue, ensureTwoPublishedLanguages);
        }

        public static string GetLocalized<T>(this T entity, Expression<Func<T, string>> keySelector,
            int languageId, bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true)
            where T:BaseEntity, ILocalizedEntity
        {
            return GetLocalized<T, string>(entity, keySelector, languageId, returnDefaultValue, ensureTwoPublishedLanguages);
        }

        /// <summary>
        /// Hàm cho phép ta lấy giá trị địa phương hóa cho 1 property của 1 thực thể entity cho trước, chẳng hạn như với 1 đối tượng 
        /// sản phẩm, ta có thể lấy ra chuỗi địa phương hóa ngôn ngữ tiếng Việt của nó cho propperty p.Name
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found). 
        /// Thiết lập là false nếu muốn hệ thống sẽ trả về rỗng nếu tài nguyên dịch cho ngôn ngữ được yêu cầu là ko có thay vì dùng
        /// giá trị của property làm giá trị thay thế</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <returns>Localized property</returns>
        public static TPropType GetLocalized<T, TPropType>(this T entity, Expression<Func<T, TPropType>> keySelector,
            int languageId, bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true)
            where T:BaseEntity, ILocalizedEntity
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var member = keySelector.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", keySelector));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", keySelector));

            TPropType result = default(TPropType);
            string resultStr = string.Empty;

            string localeKeyGroup = typeof(T).Name; // Lấy tên của lớp thực thể T làm tên nhóm localeKeyGroup
            string localeKey = propInfo.Name; // Lấy tên của property làm tên localeKey

            if(languageId > 0)
            {
                bool loadLocalizedValue = true;
                var engine = EngineContext.Current;
                if(ensureTwoPublishedLanguages)
                {
                    var languageService = engine.Resolve<ILanguageService>();
                    loadLocalizedValue = languageService.GetAllLanguages().Count >= 2;
                }

                if(loadLocalizedValue)
                {
                    var localizedEntityService = engine.Resolve<ILocalizedEntityService>();
                    resultStr = localizedEntityService.GetLocalizedValue(languageId, entity.Id, localeKeyGroup, localeKey);
                    if (!string.IsNullOrEmpty(resultStr))
                        result = CommonHelper.To<TPropType>(resultStr);
                }
            }

            if(string.IsNullOrEmpty(resultStr) && returnDefaultValue)
            {
                var localizer = keySelector.Compile();
                result = localizer(entity);
            }

            return result;
        }


        public static string GetLocalizedPermissionName(this PermissionRecord permissionRecord,
            ILocalizationService localizationService, IWorkContext workContext)
        {
            if (workContext == null) throw new ArgumentNullException("workContext");

            return GetLocalizedPermissionName(permissionRecord, localizationService, workContext.WorkingLanguage.Id);
        }

        /// <summary>
        /// Lấy về chuỗi dịch tên của 1 quyền con theo mã ngôn ngữ cho trước. Chuỗi dịch được chứa trong bảng chuỗi dịch tài nguyên
        /// thông thường LocaleStringResource
        /// </summary>
        public static string GetLocalizedPermissionName(this PermissionRecord permissionRecord,
            ILocalizationService localizationService, int languageId)
        {
            if (localizationService == null) throw new ArgumentNullException("localizationService");

            // xác định giá trị chuỗi dịch
            var resourceKey = string.Format("Permission.{0}", permissionRecord.SystemName);
            string result = localizationService.GetResource(resourceKey, languageId, false, string.Empty, true);

            return string.IsNullOrEmpty(result) ? permissionRecord.Name : result;
        }

        /// <summary>
        /// Hàm thường được gọi khi install database ở bước khởi chạy web, sẽ quét qua tất cả các ngôn ngữ đang có, lưu permissionRecord.Name
        /// vào bảng resource LocaleStringResource để làm chuỗi dịch cho PermissionRecord ở tất cả các ngôn ngữ tìm đc, đảm bảo cho
        /// PermissionRecord có được bản dịch ở tất cả các ngôn ngữ hiện hành ( ít nhất là ở thời điểm chạy code install database )
        /// </summary>
        public static void SaveLocalizedPermissionName(this PermissionRecord permissionRecord,
            ILocalizationService localizationService, ILanguageService languageService, bool saveChangeAndPublishEvent = true)
        {
            if (localizationService == null) throw new ArgumentNullException("localizationService");
            if (languageService == null) throw new ArgumentNullException("languageService");

            var resourceKey = string.Format("Permission.{0}", permissionRecord.SystemName);
            // duyệt qua tất cả các ngôn ngữ và ghi chuỗi dịch cho tên của PermissionRecord ở tất cả các ngôn ngữ đó
            foreach(var language in languageService.GetAllLanguages(showHidden: true))
            {
                var lsr = localizationService.GetLocaleStringResourceByName(resourceKey, language.Id, false);
                if(lsr == null)
                {
                    localizationService.InsertLocaleStringResource(new LocaleStringResource
                    {
                        LanguageId = language.Id,
                        ResourceName = resourceKey,
                        ResourceValue = permissionRecord.Name
                    }, false);
                }else
                {
                    lsr.ResourceValue = permissionRecord.Name;
                    localizationService.UpdateLocaleStringResource(lsr, false);
                }
            }
            if (saveChangeAndPublishEvent) localizationService.SaveChange();
        }

        /// <summary>
        /// Hàm xóa chuỗi dịch tài nguyên cho permissionRecord, vô cùng ít khi dùng
        /// </summary>
        public static void DeleteLocalizedPermissionName(this PermissionRecord permissionRecord,
            ILocalizationService localizationService, ILanguageService languageService, bool saveChangeAndPublishEvent = true)
        {
            if (localizationService == null) throw new ArgumentNullException("localizationService");
            if (languageService == null) throw new ArgumentNullException("languageService");

            var resourceKey = string.Format("Permission.{0}", permissionRecord.SystemName);
            // duyệt qua tất cả các ngôn ngữ và ghi chuỗi dịch cho tên của PermissionRecord ở tất cả các ngôn ngữ đó
            foreach (var language in languageService.GetAllLanguages(showHidden: true))
            {
                var lsr = localizationService.GetLocaleStringResourceByName(resourceKey, language.Id, false);
                if (lsr != null) localizationService.DeleteLocaleStringResource(lsr, false);
            }
            if (saveChangeAndPublishEvent) localizationService.SaveChange();
        }
    }
}
