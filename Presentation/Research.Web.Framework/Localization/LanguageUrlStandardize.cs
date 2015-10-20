using Research.Core;
using Research.Core.Domain.Localization;
using System.Web;

namespace Research.Web.Framework.Localization
{
    public class LanguageUrlStandardize : ILanguageUrlStandardize
    {
        private readonly HttpContextBase _httpContext;
        private readonly LocalizationSettings _localizationSettings;

        public LanguageUrlStandardize(HttpContextBase httpContext,
            LocalizationSettings localizationSettings)
        {
            _httpContext = httpContext;
            _localizationSettings = localizationSettings;
        }

        /// <summary>
        /// Cần đc gọi 1 lần duy nhất trong Begin_Request. Ghi chú là hàm sẽ ko làm bất cứ điều gì nếu hệ thống ko hỗ trợ phân đoạn
        /// ngôn ngữ
        /// 
        /// [[[Trong trường hợp hệ thống dùng url có phân đoạn ngôn ngữ]]], hàm sẽ gọi RewritePath để đảm bảo 
        /// AppRelativeCurrentExecutionFilePath bị loại bỏ phân đoạn ngôn ngữ nếu có
        /// </summary>
        public void RewriteAppRelativeCurrentExecutionFilePath()
        {
            if (!_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled) return;

            var request = _httpContext.Request;
            string rawUrl = request.RawUrl;
            string applicationPath = request.ApplicationPath;
            if (rawUrl.IsLocalizedUrl(applicationPath, true)) 
            {
                var newVirtualPath = rawUrl.RemoveLanguageSeoCodeFromRawUrl(applicationPath);
                if (string.IsNullOrEmpty(newVirtualPath)) newVirtualPath = "/";
                newVirtualPath = newVirtualPath.RemoveApplicationPathFromRawUrl(applicationPath);
                newVirtualPath = "~" + newVirtualPath;
                _httpContext.RewritePath(newVirtualPath, true);
            }
        }
    }
}
