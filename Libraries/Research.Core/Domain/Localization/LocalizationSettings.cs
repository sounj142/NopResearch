using Research.Core.Configuration;

namespace Research.Core.Domain.Localization
{
    /// <summary>
    /// Ghi chú: Hiện nay cơ chế cache cho setting là cache static, nhưng cơ chế cache cho các đối tượng ISettings lại ko có, điều đó
    /// đồng nghĩa với việc mỗi một request, các đối tượng ISettings nếu cần dùng sẽ phải đọc lại 1 lần từ static cache thông qua cơ chế
    /// reflection
    /// 
    /// Cần xem xét tăng hiệu năng bằng cách cache tất cả các đối tượng ISetting vào static cache, và sẽ clear các đối tượng này
    /// một khi có bất kỳ thay đổi gì trên bảng setting ( sự kiện insert/update/delete/all trên settings )
    /// </summary>
    public partial class LocalizationSettings: ISettings
    {
        /// <summary>
        /// Default admin area language identifier
        /// </summary>
        public int DefaultAdminLanguageId { get; set; }

        /// <summary>
        /// Use images for language selection
        /// </summary>
        public bool UseImagesForLanguageSelection { get; set; }

        /// <summary>
        /// A value indicating whether SEO friendly URLs with multiple languages are enabled
        /// </summary>
        public bool SeoFriendlyUrlsForLanguagesEnabled { get; set; }

        /// <summary>
        /// A value indicating whether we should detect the current language by a customer region (browser settings)
        /// </summary>
        public bool AutomaticallyDetectLanguage { get; set; }

        /// <summary>
        /// A value indicating whether to load all LocaleStringResource records on application startup
        /// </summary>
        public bool LoadAllLocaleRecordsOnStartup { get; set; }

        /// <summary>
        /// A value indicating whether to load all LocalizedProperty records on application startup
        /// </summary>
        public bool LoadAllLocalizedPropertiesOnStartup { get; set; }

        /// <summary>
        /// A value indicating whether to load all search engine friendly names (slugs) on application startup
        /// </summary>
        public bool LoadAllUrlRecordsOnStartup { get; set; }

        /// <summary>
        /// A value indicating whether to we should ignore RTL language property for admin area
        /// </summary>
        public bool IgnoreRtlPropertyForAdminArea { get; set; }

        /// <summary>
        /// Qui định thời gian tính theo giờ để 1 chuỗi slug có thể được lưu giữ lại dưới dạng unactive, tức là khi người dùng 
        /// đổi Url SEO của 1 sản phẩm chẳng hạn, khi đó tùy theo thời gian tồn tại, cái Url cũ có thể bị thay thế hoàn toàn, hoặc sẽ
        /// được chuyển trạng thái sang unactive để đảm bảo có thể duy trì được cho những người dùng đã bookmark link cũ
        /// 
        /// Nếu là 0 thì sẽ luôn luôn lưu giữ link cũ
        /// </summary>
        public int MinHoursForUrlSlugEverlasting { get; set; }
    }
}
