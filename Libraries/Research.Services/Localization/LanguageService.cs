using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Localization;
using Research.Core.Interface.Service;
using Research.Core.Interface.Data;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using System;
using Research.Services.Caching.Models;

namespace Research.Services.Localization
{
    /// <summary>
    /// Đã sửa lại code để tập trung cache 1 lần tất cả các language theo static cache, 
    /// và cache get by Id, get all by show hidden records theo per request cache
    /// </summary>
    public partial class LanguageService: BaseService<Language>, ILanguageService
    {
        #region Fields, properties and ctors

        // Ta sẽ sửa lại code để tập trung vào dùng static cache

        private readonly ILanguageCacheWriter _cacheWriter;
        private readonly ISettingService _settingService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly LocalizationSettings _localizationSettings;

        public LanguageService(IRepository<Language> repository, 
            ILanguageCacheWriter languageCache,
            ISettingService settingService,
            IStoreMappingService storeMappingService,
            LocalizationSettings localizationSettings,
            IEventPublisher eventPublisher)
            : base(repository, eventPublisher)
        {
            _cacheWriter = languageCache;
            _settingService = settingService;
            _localizationSettings = localizationSettings;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Lấy về list tất cả các language, được cache static. Đây là cache language duy nhất ở mức static, các cache khác sẽ ở mức
        /// per request
        /// 
        /// Cần chú ý clone lại tất cả các data đọc được từ cache trước khi dùng, để tránh thao tác sai khi tự ý thay đổi trên các
        /// đối tượng khi mà nó lại nằm trong cache. Sẽ tệ hại nhất khi thao tác thay đổi này nằm ở đâu đó trong font-end, hoặc
        /// thao tác thay đổi nhưng ko commit tran/commit tran ko thành công
        /// </summary>
        /// <returns></returns>
        protected virtual IList<LanguageForCache> GetAllLanguagesCached()
        {
            return _cacheWriter.GetAll(() => {
                var result = new List<LanguageForCache>();
                foreach(var language in _repository.TableNoTracking.OrderBy(p => p.DisplayOrder).ToList())
                {
                    result.Add(LanguageForCache.Transform(language));
                }
                return result;
            });
        }

        public virtual IList<Language> GetAllLanguages(bool getFromStaticCache = true, bool showHidden = false, int storeId = 0)
        {
            return _cacheWriter.GetAll(showHidden, storeId, getFromStaticCache, () =>
            {
                if(getFromStaticCache)
                {
                    IEnumerable<LanguageForCache> allLanguages = GetAllLanguagesCached();
                    if (!showHidden) allLanguages = allLanguages.Where(p => p.Published);

                    if (storeId > 0)
                    {
                        // lưu ý là vì có liên quan đến store mapping ở đây nên khi có thay đổi ở bảng StoreMapping, cần phải clear
                        // ở những cache như thế này để đảm bảo tính chính xác, đặc biệt là nếu dùng static cache
                        return allLanguages
                            .Select(p => LanguageForCache.Transform(p))
                            .Where(l => _storeMappingService.Authorize(l, storeId))
                            .ToList();
                    }
                    return allLanguages.Select(p => LanguageForCache.Transform(p)).ToList();
                }
                else
                {
                    IQueryable<Language> allLanguages = _repository.Table;
                    if (!showHidden) allLanguages = allLanguages.Where(p => p.Published);
                    allLanguages = allLanguages.OrderBy(p => p.DisplayOrder);
                    if (storeId > 0)
                    {
                        return allLanguages.ToList()
                            .Where(l => _storeMappingService.Authorize(l, storeId)).ToList();
                    }
                    return allLanguages.ToList();
                }
            });
        }

        public override void Delete(Language language)
        {
            if (language == null) throw new ArgumentNullException("language");

            // nếu xóa phải ngôn ngữ mặc định của giao diện admin ?
            if(_localizationSettings.DefaultAdminLanguageId == language.Id)
            {
                var newLang = GetAllLanguagesCached().FirstOrDefault(p => p.Id != language.Id);
                if(newLang != null)
                {
                    _localizationSettings.DefaultAdminLanguageId = newLang.Id;
                    _settingService.SaveSetting(_localizationSettings, p => p.DefaultAdminLanguageId); // chỉ save 1 property thay vì toàn bộ đối tượng
                    //_settingService.SaveSetting(_localizationSettings); 
                }
            }

            base.Delete(language);
        }

        /// <summary>
        /// Đã overide để get từ static cache và lưu vào perrequest cache
        /// </summary>
        public virtual Language GetById(int id, bool getFromStaticCache)
        {
            return _cacheWriter.Get(id, getFromStaticCache, () => {
                if (getFromStaticCache)
                    return LanguageForCache.Transform(GetAllLanguagesCached().FirstOrDefault(l => l.Id == id));
                else
                {
                    return _repository.GetById(id);
                }
            }); 
        }

        #endregion
    }
}
