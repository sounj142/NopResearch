using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Localization;
using Research.Core.Interface.Service;
using Research.Core.Interface.Data;
using Research.Core;
using Research.Services.Caching.Writer;
using Research.Core.Domain.Common;
using Research.Services.Events;
using Research.Services.Logging;

namespace Research.Services.Localization
{
    /// <summary>
    /// Quản lý đọc, thêm, xóa, sửa, ghi excel các chuỗi Resource của hệ thống ( thường là các chuỗi dịch dùng trong view )
    /// Cơ chế cache là static cache dựa trên 1 hoặc 2 dạng sau
    /// + Cache tất cả dùng 1 dictionary duy nhất
    /// + Cache riêng từng key, chỉ cache những key được truy xuất tới, những key ko truy xuất thì ko cache
    /// 
    /// Vì chúng ta chỉ cache [tên resource - language id] => value nên giá trị cache ko có ý nghĩa trong các thao tác trên đối tượng
    /// LocaleStringResource => Tất cả mọi thao tác trên đối tượng này đều làm trực tiếp với database như thông thường,
    /// chỉ có thao tác đọc giá trị value string theo key mới dùng cache
    /// 
    /// 
    /// Ghi chú: Khóa resourceName trong resource là ko phân biệt hoa thường
    /// </summary>
    public partial class LocalizationService : BaseService<LocaleStringResource>, ILocalizationService
    {
        #region fields, property, ctor

        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationCacheWriter _cacheWriter;
        private readonly CommonSettings _commonSettings;
        private readonly LocalizationSettings _localizationSettings;

        protected ILocalizationRepository Repository
        {
            get { return (ILocalizationRepository)_repository; }
        }

        public LocalizationService(ILocalizationRepository repository,
            IWorkContext workContext,
            ILogger logger,
            ILanguageService languageService,
            ILocalizationCacheWriter cacheWriter,
            CommonSettings commonSettings,
            LocalizationSettings localizationSettings,
            IEventPublisher eventPublisher)
            : base(repository, eventPublisher)
        {
            _workContext = workContext;
            _logger = logger;
            _languageService = languageService;
            _cacheWriter = cacheWriter;
            _commonSettings = commonSettings;
            _localizationSettings = localizationSettings;
        }
        #endregion

        #region methods

        /// <summary>
        /// Hàm đọc resource từ database như thông thường
        /// </summary>
        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName)
        {
            // nếu như ngôn ngữ hiện hành khác null thì lấy theo ngôn ngữ hiện hành
            var workingLanguage = _workContext.WorkingLanguage;
            if (workingLanguage != null)
                return GetLocaleStringResourceByName(resourceName, workingLanguage.Id);
            return null;
        }

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <returns>Locale string resource</returns>
        public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId, bool logIfNotFound = true)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                if (logIfNotFound)
                    _logger.Warning("GetLocaleStringResourceByName(): Cố gắng truy cập resource với khóa null/empty");
                return null;
            }
            var locale = _repository.Table.FirstOrDefault(p => p.ResourceName == resourceName &&
                p.LanguageId == languageId);
            if (locale == null && logIfNotFound)
                _logger.Warning(string.Format("GetLocaleStringResourceByName(): Resource string ({0}) not found. Language ID = {1}", resourceName, languageId));

            return locale;
        }

        public virtual IList<LocaleStringResource> GetAll(int languageId)
        {
            return GetAllQueryable(languageId).ToList();
        }

        public virtual IQueryable<LocaleStringResource> GetAllQueryable(int languageId)
        {
            return _repository.Table.Where(p => p.LanguageId == languageId).OrderBy(l => l.ResourceName);
        }


        //  Sửa: Đã ẩn hàm khỏi interface để ngăn bên ngoài tự ý gọi vào
        public virtual IDictionary<string, KeyValuePair<int, string>> GetAllResourceValues(int languageId)
        {
            return _cacheWriter.GetAllByLaguageId(languageId, () => {
                var dict = new Dictionary<string, KeyValuePair<int, string>>();
                var query = _repository.TableNoTracking
                    .Where(p => p.LanguageId == languageId)
                    .OrderBy(p => p.ResourceName);
                foreach (var resource in query)
                    dict[resource.ResourceName.ToLowerInvariant()] = 
                        new KeyValuePair<int, string>(resource.Id, resource.ResourceValue);

                return dict;
            });
        }

        /// <summary>
        /// Lấy từ static cache ra theo 1 trong 2 cơ chế: Cache All hoặc cache từng cái theo key, tùy theo cấu hình của website
        /// </summary>
        public virtual string GetResource(string resourceKey, bool logIfNotFound = true)
        {
            var workingLanguage = _workContext.WorkingLanguage;
            if (workingLanguage != null)
                return GetResource(resourceKey, workingLanguage.Id, logIfNotFound);
            return string.Empty;
        }

        public virtual string GetResource(string resourceKey, int languageId, bool logIfNotFound = true, 
            string defaultValue = "", bool returnEmptyIfNotFound = false)
        {
            string result = null;
            if (string.IsNullOrEmpty(resourceKey))
            {
                if (logIfNotFound)
                    _logger.Warning("GetResource(): Cố gắng truy cập resource với khóa null/empty");
                return string.Empty;
            }
            resourceKey = resourceKey.ToLowerInvariant();
            if(_localizationSettings.LoadAllLocaleRecordsOnStartup)
            {
                // nếu cấu hình web là load tất cả resource vào static cache
                var dict = GetAllResourceValues(languageId);
                KeyValuePair<int, string> value;
                if (dict.TryGetValue(resourceKey, out value)) result = value.Value;
            }
            else
            {
                // cấu hình website là cache từng resource riêng lẻ
                result = _cacheWriter.GetByResourceKeyAndLanguageId(resourceKey, languageId, () => {
                    return _repository.TableNoTracking
                        .Where(p => p.LanguageId == languageId && p.ResourceName == resourceKey)
                        .Select(p => p.ResourceValue).FirstOrDefault();
                });
            }
            if (result == null) // ko tìm thấy hoặc dữ liệu trong bảng resource sai
            {
                if (logIfNotFound)
                    _logger.Warning(string.Format("GetResource(): Resource string ({0}) is not found. Language ID = {1}", resourceKey, languageId));
                if (!string.IsNullOrEmpty(defaultValue))
                    result = defaultValue;
                else{
                    if (!returnEmptyIfNotFound)
                        result = resourceKey; // trả về tên khóa. Đây chính là lý do mà khi ta thêm 1 ngôn ngữ mới ( nhưng thiếu resource )
                    // thì giao diện ngôn ngữ mới tràn ngập những khóa resource.xxx.yyy. Điều này sẽ giúp ích cho việc dịch resource của người
                    // quản trị trang web
                }
            }

            return result;
        }
        
        /// <summary>
        /// Chưa cài đặt
        /// </summary>
        public virtual string ExportResourcesToXml(Language language)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Chưa cài đặt
        /// </summary>
        public virtual void ImportResourcesFromXml(Language language, string xml)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteLocaleStringResource(LocaleStringResource entity, bool saveChange = true)
        {
            Delete(entity, saveChange, saveChange);
        }

        public void InsertLocaleStringResource(LocaleStringResource entity, bool saveChange = true)
        {
            Insert(entity, saveChange, saveChange);
        }

        public void UpdateLocaleStringResource(LocaleStringResource entity, bool saveChange = true)
        {
            Update(entity, saveChange, saveChange);
        }

        public void SaveChange()
        {
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityAllChange((LocaleStringResource)null);
        }

        #endregion

    }
}
