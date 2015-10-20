using Research.Core.Configuration;
using System.Collections.Generic;

namespace Research.Core.Domain.Common
{
    public class CommonSettings: ISettings
    {
        public CommonSettings()
        {
            IgnoreLogWordlist = new List<string>();
        }

        public bool UseSystemEmailForContactUsForm { get; set; }

        public bool UseStoredProceduresIfSupported { get; set; }

        public bool HideAdvertisementsOnAdminArea { get; set; }

        public bool SitemapEnabled { get; set; }
        public bool SitemapIncludeCategories { get; set; }
        public bool SitemapIncludeManufacturers { get; set; }
        public bool SitemapIncludeProducts { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether to display a warning if java-script is disabled
        /// </summary>
        public bool DisplayJavaScriptDisabledWarning { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether full-text search is supported
        /// </summary>
        public bool UseFullTextSearch { get; set; }

        /// <summary>
        /// Gets a sets a Full-Text search mode
        /// </summary>
        public FulltextSearchMode FullTextMode { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether 404 errors (page or file not found) should be logged
        /// </summary>
        public bool Log404Errors { get; set; }

        /// <summary>
        /// Gets a sets a breadcrumb delimiter used on the site
        /// </summary>
        public string BreadcrumbDelimiter { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether we should render <meta http-equiv="X-UA-Compatible" content="IE=edge"/> tag
        /// </summary>
        public bool RenderXuaCompatible { get; set; }

        /// <summary>
        /// Gets a sets a value of "X-UA-Compatible" META tag
        /// </summary>
        public string XuaCompatibleValue { get; set; }

        /// <summary>
        /// Gets or sets a ignore words (phrases) to be ignored when logging errors/messages
        /// Sẽ được convert thông qua TypeConvert đặc biệt được cài đặt để convert qua lại 
        /// giữa IList và 1 chuỗi các giá trị cách nhau bởi dấu phẩy
        /// </summary>
        public List<string> IgnoreLogWordlist { get; set; }

        /// <summary>
        /// ( Tự đinh nghĩa ): Thời gian tính bằng phút mà cookie lưu giữ Guest Guid tồn tại
        /// </summary>
        public int GuestGuidCookiesExpiresMinutes { get; set; }

        /// <summary>
        /// ( Tự đinh nghĩa ): Chiều dài tối đa của 1 phần tử trong cột Comment của bảng ActivityLog ( nếu vượt quá sẽ bị cắt bớt )
        /// </summary>
        public int ActivityLogCommentMaxLength { get; set; }
    }
}
