using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Research.Core.Domain.Localization;
using Research.Core.Interface.Service;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using Research.Core.Interface.Data;
using Research.Core;

namespace Research.Services.Localization
{
    public partial class LocalizedEntityService : BaseService<LocalizedProperty>, ILocalizedEntityService
    {
        #region Fields, Properties, Ctors

        private readonly ILocalizedEntityCacheWriter _cacheWriter;
        private readonly LocalizationSettings _localizationSettings;

        public LocalizedEntityService(IRepository<LocalizedProperty> repository,
            ILocalizedEntityCacheWriter cacheWriter,
            IEventPublisher eventPublisher,
            LocalizationSettings localizationSettings)
            : base(repository, eventPublisher)
        {
            _cacheWriter = cacheWriter;
            _localizationSettings = localizationSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Lấy về tất cả các localized properties của đối tượng với tên localeKeyGroup và id là entityId, ko dùng cache
        /// </summary>
        protected virtual IList<LocalizedProperty> GetLocalizedProperties(int entityId, string localeKeyGroup)
        {
            if (entityId <= 0 || string.IsNullOrEmpty(localeKeyGroup))
                return new List<LocalizedProperty>();

            return _repository.Table
                .Where(p => p.EntityId == entityId && p.LocaleKeyGroup == localeKeyGroup)
                .OrderBy(p => p.Id)
                .ToList();
        }

        /// <summary>
        /// Trả về danh sách các LocalizedProperty đang có được tổ chức lại dưới dạng 1 dict. Vì nhu cầu chỉ là tìm và sử dụng 
        /// chuỗi LocaleValue nên chúng ta sẽ chỉ cache trong từ điển duy nhất chuỗi LocaleValue với khóa là chuỗi kết hợp 4 key
        /// </summary>
        protected virtual IDictionary<string,string> GetAllLocalizedPropertiesCached()
        {
            return _cacheWriter.GetAll(() => {
                var dict = new Dictionary<string, string>();
                foreach (var lp in _repository.TableNoTracking)
                {
                    string key = string.Format("{0}-{1}-{2}-{3}", lp.LanguageId, lp.EntityId, lp.LocaleKeyGroup, lp.LocaleKey);
                    dict[key] = lp.LocaleValue;
                }
                    
                return dict;
            });
        }

        #endregion

        #region Methods

        public virtual IList<LocalizedProperty> GetLocalizedProperties<T>(T entity)
             where T : BaseEntity, ILocalizedEntity
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return GetLocalizedProperties(entity.Id, typeof(T).Name);
        }

        public virtual string GetLocalizedValue(int languageId, int entityId, string localeKeyGroup, string localeKey)
        {
            if(_localizationSettings.LoadAllLocalizedPropertiesOnStartup)
            {
                var allLocalized = GetAllLocalizedPropertiesCached();
                string key = string.Format("{0}-{1}-{2}-{3}", languageId, entityId, localeKeyGroup, localeKey);
                string value;
                return allLocalized.TryGetValue(key, out value) ? value : string.Empty;
            }else
            {
                // chỉ cache riêng từng key mỗi khi cần đến
                return _cacheWriter.Get(languageId, entityId, localeKeyGroup, localeKey, () => {
                    var localizeValue = _repository.TableNoTracking
                        .Where(p => p.EntityId == entityId &&
                        p.LanguageId == languageId && p.LocaleKeyGroup == localeKeyGroup && p.LocaleKey == localeKey)
                        .Select(p => p.LocaleValue).FirstOrDefault();

                    return localizeValue ?? string.Empty; // nếu ko thấy thì cache "" vào để đánh dâu là ko thấy
                    // để tránh truy cập CSDL nhiều lần làm giảm hiệu năng. Để làm điều này thì bộ 4 khóa phải đến từ 1 nguốn đáng tin cậy
                    // chẳng hạn như nội tại bên trong code, chứ nếu đến từ nguồn ko tin cậy thì có thể dẫn tới cache rác quá nhiều gây
                    // tràn bộ nhớ
                });
            }
        }

        public virtual void SaveLocalizedValue<T>(T entity, Expression<Func<T, string>> keySelector, 
            string localeValue, int languageId) where T : BaseEntity, ILocalizedEntity
        {
            SaveLocalizedValue<T, string>(entity, keySelector, localeValue, languageId);
        }

        public virtual void SaveLocalizedValue<T, TPropType>(T entity, Expression<Func<T, TPropType>> keySelector,
            TPropType localeValue, int languageId) where T : BaseEntity, ILocalizedEntity
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (languageId <= 0) throw new ArgumentOutOfRangeException("languageId", "Language ID should greater than 0");

            var member = keySelector.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", keySelector));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", keySelector));

            // sẽ ko cần dùng GetUnproxiedEntityType() ở đây vì dù entity có là 1 đối tượng proxy thì T vẫn có kiểu là kiểu cơ sở
            // của nó. Nên nhớ, tất cả những đối tượng do EF trả về đều là kiểu cơ sở ( dù thực tế nó là kiểu proxy ). Việc gửi
            // entity vào như là tham số cho hàm sẽ khiến C# xác định T là kiểu cơ sở và hiểu đúng
            string localeKeyGroup = typeof(T).Name; // Lấy tên của lớp thực thể T làm tên nhóm localeKeyGroup
            string localeKey = propInfo.Name; // Lấy tên của property làm tên localeKey

            // Lấy về tất cả các localized properties của đối tượng với tên localeKeyGroup và id là entityId, ko dùng cache
            var props = GetLocalizedProperties(entity.Id, localeKeyGroup);
            var prop = props.FirstOrDefault(p => p.LanguageId == languageId &&
                string.Equals(p.LocaleKey, localeKey, StringComparison.InvariantCulture));
            var localeValueStr = CommonHelper.To<string>(localeValue);

            if (prop != null) // đã tìm thấy 1 LocalizedProperty có sẵn trùng với chuỗi cần thêm
            {
                if (string.IsNullOrWhiteSpace(localeValueStr))// nếu chuỗi mới muốn thêm là rỗng thì xóa bỏ nó đi, ngay và luôn
                    Delete(prop);
                else
                {
                    prop.LocaleValue = localeValueStr;
                    Update(prop);
                }
            }
            else
            {
                // ko tìm thấy, thêm mới nếu cần thiết
                if (!string.IsNullOrWhiteSpace(localeValueStr))
                {
                    Insert(new LocalizedProperty
                    {
                        EntityId = entity.Id,
                        LanguageId = languageId,
                        LocaleKey = localeKey,
                        LocaleKeyGroup = localeKeyGroup,
                        LocaleValue = localeValueStr
                    });
                }
            }
        }

        #endregion
    }
}
