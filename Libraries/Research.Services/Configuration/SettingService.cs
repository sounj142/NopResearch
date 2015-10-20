using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Research.Core.Domain.Configuration;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Events;
using Research.Services.Caching.Writer;
using Research.Services.Caching.Models;
using Research.Core;
using Research.Core.Configuration;
using System.ComponentModel;
using System.Reflection;

namespace Research.Services.Configuration
{
    /// <summary>
    /// Sẽ sử dụng hoàn toàn là static cache
    /// </summary>
    public class SettingService : BaseService<Setting>, ISettingService
    {
        #region fields, properties, ctors

        
        private readonly ISettingCacheWriter _cacheWriter;

        public SettingService(IRepository<Setting> repository, 
            IEventPublisher eventPublisher, 
            ISettingCacheWriter cacheWriter)
            : base(repository, eventPublisher)
        {
            _cacheWriter = cacheWriter;
        }

        #endregion


        #region Utilities

        /// <summary>
        /// Hàm đọc tất cả các thông tin cấu hình trong database vào 1 từ điển, trong đó 1 key sẽ được liên kết với 1 IList có thể
        /// có 1 hoặc nhiều value trong đó ( ứng với nhiều storeId )
        /// Kết quả trả về của hàm sẽ được static cache để sử dụng trong tất cả các thao tác có liên quan, tức là thao tác đọc database sẽ đc
        /// thực hiện chỉ 1 lần
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, SettingForCache> GetAllSettingsCached()
        {
            return _cacheWriter.GetAll(() => {
                var dict = new Dictionary<string, SettingForCache>();
                // sử dụng no tracking để tăng hiệu năng khi các dữ liệu lấy về được sử dụng với mục đích chỉ đọc
                var settings = _repository.TableNoTracking; //.OrderBy(p => p.Name).ThenBy(p => p.StoreId); // làm thao tác firstordefault(store = 0) nhanh hơn

                foreach (var s in settings)
                    dict[s.Name + s.StoreId] = SettingForCache.Transform(s);
                return dict;
            });
        }

        #endregion

        #region methods

        /// <summary>
        /// Dùng hàm này thay cho Insert() của lớp base vì nó có hỗ trợ cho clear cache
        /// </summary>
        public virtual void InsertSetting(Setting setting, bool saveChange = true)
        {
            setting.Name = setting.Name.Trim().ToLowerInvariant(); // đảm bảo là Name dùng ký tự in thường
            if(saveChange)
            {
                base.Insert(setting);
            }else
            {
                _repository.Insert(setting); // chỉ đưa đối tượng vào dbcontext chứ ko hề savechange hay phát sự kiện
            }
        }

        /// <summary>
        /// Dùng hàm này thay cho base.Update vì nó có hỗ trợ clear cache
        /// </summary>
        public virtual void UpdateSetting(Setting setting, bool saveChange = true)
        {
            setting.Name = setting.Name.Trim().ToLowerInvariant(); // đảm bảo là Name dùng ký tự in thường
            if (saveChange)
            {
                base.Update(setting);
            }
            else
            {
                _repository.Update(setting); 
            }
        }

        /// <summary>
        /// Dùng thay cho delete
        /// </summary>
        public virtual void DeleteSetting(Setting setting, bool saveChange = true)
        {
            if (saveChange)
            {
                base.Delete(setting);
            }
            else
            {
                _repository.Delete(setting);
            }
        }
        /// <summary>
        /// override lại để lấy obj từ cache. Chú ý là đối tượng trả về từ hàm này là đối tượng setting được tạo mới => cần để ý
        /// các thao tác update, delete xem có hoạt động đúng hay ko
        /// 
        /// Đã test: Các thao tác insert, delete nhờ đã gọi Attach nên hoạt động bình thường, chả có vấn đề gì cả
        /// 
        /// 
        /// Cần phải xem lại phạm vi sử dụng của hàm này. Nếu hàm được gọi tới nhiều lần thì nên cân nhắc 2 phương án
        /// + Đọc trực tiếp database ko qua cache như nop làm
        /// + Tổ chức thêm 1 List đã sắp sếp của Setting theo Id => Khi đó việc tìm kiếm qui về tìm nhị phân. Cũng có thể tổ chức
        /// setting ko theo IDict mà sắp xếp theo name, khi đó cũng có thể dùng tìm kiếm nhị phân
        /// 
        /// Đã kiểm tra: Hàm này rất ít dùng, gần như chỉ gọi cực ít khi save setting trong admin
        /// </summary>
        public virtual Setting GetById(int id, bool getFromStaticCache)
        {
            if (id <= 0) return null;
            if (getFromStaticCache)
            {
                var allSettings = GetAllSettingsCached();
                foreach (var setting in allSettings.Values)
                    if (setting.Id == id)
                        return SettingForCache.Transform(setting);
            }
            else return _repository.GetById(id);
            
            return null;
        }
        /// <summary>
        /// Hàm tìm kiếm từ static cache
        /// </summary>
        protected virtual SettingForCache Search(string key, int storeId = 0, bool loadSharedValueIfNotFound = false)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            var settings = GetAllSettingsCached();
            key = key.Trim().ToLowerInvariant();
            SettingForCache result;
            if (settings.TryGetValue(key + storeId, out result) ||
                (loadSharedValueIfNotFound && storeId > 0 && settings.TryGetValue(key + "0", out result)))
                return result;
            return null;
        }

        /// <summary>
        /// Get setting by key
        /// 
        /// Hàm này hình như ko thấy dùng đến ?
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all stores) value should be loaded if a value specific for a certain is not found</param>
        /// <returns>Setting</returns>
        public virtual Setting GetSetting(string key, int storeId = 0, bool loadSharedValueIfNotFound = false, bool getFromStaticCache = true)
        {
            var cacheSetting = Search(key, storeId, loadSharedValueIfNotFound);
            if (cacheSetting == null) return null;
            if (getFromStaticCache)
                return SettingForCache.Transform(cacheSetting);
            else return GetById(cacheSetting.Id, false);
        }

        /// <summary>
        /// Lấy về giá trị Value ứng với khóa key, sẽ tự động Convert kiểu chuỗi mà Value chứa thành kiểu T để trả về
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all stores) value should be loaded if a value specific for a certain is not found</param>
        /// <returns>Setting value</returns>
        public virtual T GetSettingByKey<T>(string key, T defaultValue = default(T), int storeId = 0, bool loadSharedValueIfNotFound = false)
        {
            var setting = Search(key, storeId, loadSharedValueIfNotFound);
            if (setting == null) return defaultValue;
            return CommonHelper.To<T>(setting.Value);
        }

        public virtual void SetSetting<T>(string key, T value, int storeId = 0, bool saveChange = true)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key");

            string newValue = CommonHelper.GetNopCustomTypeConverter(typeof(T)).ConvertToInvariantString(value);
            var settingForCaching = Search(key, storeId, false);
            if (settingForCaching != null)
            {
                // đã có key, tiến hành update
                var setting = SettingForCache.Transform(settingForCaching);
                setting.Value = newValue;
                UpdateSetting(setting, saveChange);
            }
            else
            {
                // chưa có, insert mới vào database
                var setting = new Setting
                {
                    Name = key,
                    StoreId = storeId,
                    Value = newValue
                };
                InsertSetting(setting, saveChange);
            }
        }

        public virtual IList<Setting> GetAllSettings()
        {
            var settings = GetAllSettingsCached();
            return settings.Values.OrderBy(p => p.Name).ThenBy(p => p.StoreId)
                .Select(p => new Setting
                {
                    Id = p.Id,
                    Name = p.Name,
                    Value = p.Value,
                    StoreId = p.StoreId
                }).ToList();
        }

        public virtual bool SettingExists<T, TPropType>(T settings, Expression<Func<T, TPropType>> keySelector, int storeId)
            where T : ISettings, new()
        {
            return GetSettingByKey<string>(key: settings.GetSettingKey(keySelector), storeId: storeId) != null;
        }

        /// <summary>
        /// Đọc dữ liệu từ bảng Settings lên để tạo ra đối tượng setting có kiểu type
        /// Đã sử lại để lưu cache static
        /// </summary>
        public virtual object LoadSetting(Type type, int storeId = 0)
        {
            object result;
            // kiểm tra trước = TryGetSetting để đưa hàm GetAllSettingsCached(); ra khỏi aquire(), tránh 2 yêu cầu cache static lồng nhau
            // ko cần thiết
            if (_cacheWriter.TryGetSetting(storeId, type, out result))
                return result;
            var allSettings = GetAllSettingsCached();

            return _cacheWriter.GetSetting(storeId, type, () =>
            {
                if (!typeof(ISettings).IsAssignableFrom(type)) return null;
                if (type.GetConstructor(Type.EmptyTypes) == null) return null;
                var settings = Activator.CreateInstance(type);
                if (settings == null) return null;

                var convertDict = new Dictionary<string, TypeConverter>();
                SettingForCache settingObj;
                foreach (var prop in type.GetProperties())
                    if (prop.CanRead && prop.CanWrite)
                    {
                        var key = (type.Name + "." + prop.Name).ToLowerInvariant();
                        if (!(allSettings.TryGetValue(key + storeId, out settingObj) ||
                            (storeId > 0 && allSettings.TryGetValue(key + "0", out settingObj))))
                            continue;

                        TypeConverter converter;
                        string propFullName = prop.PropertyType.FullName;
                        if (!convertDict.TryGetValue(propFullName, out converter))
                        {
                            convertDict[propFullName] = converter = CommonHelper.GetNopCustomTypeConverter(prop.PropertyType);
                        }
                        if (converter == null) continue;
                        if (!converter.CanConvertFrom(typeof(string))) continue;
                        if (!converter.IsValid(settingObj.Value)) continue;

                        prop.SetValue(settings, converter.ConvertFromInvariantString(settingObj.Value));
                    }

                return settings;
            });
        }

        /// <summary>
        /// Đọc dữ liệu từ bảng Setting và load lên cho tất cả các property có thể có của kiểu T
        /// Đã sửa để lưu staticCache
        /// </summary>
        public virtual T LoadSetting<T>(int storeId = 0) where T : ISettings, new()
        {
            return (T)LoadSetting(typeof(T), storeId);
        }

        /// <summary>
        /// Lưu tất cả các property mà đối tượng settings chứa vào bảng Setting
        /// </summary>
        public virtual void SaveSetting<T>(T settings, int storeId = 0, bool saveChange = true) where T : ISettings, new()
        {
            var typeOfT = typeof(T);
            var convertDict = new Dictionary<string, TypeConverter>();
            foreach(var prop in typeOfT.GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                TypeConverter converter;
                string propFullName = prop.PropertyType.FullName;
                if (!convertDict.TryGetValue(propFullName, out converter))
                {
                    convertDict[propFullName] = converter = CommonHelper.GetNopCustomTypeConverter(prop.PropertyType);
                }
                if (converter == null) continue;
                if(!converter.CanConvertFrom(typeof(string))) continue;
                
                var key = typeOfT.Name + "." + prop.Name;
                object value = prop.GetValue(settings);
                if (value != null)
                    SetSetting(key, value, storeId, false);
                else SetSetting(key, string.Empty, storeId, false); // tại sao lại truyền chuỗi rỗng ? Chuỗi rỗng sẽ được convert thành
                // giá trị null hay sao ? Hay là do qui ước cột value trong bảng Setting ko được phép null ?
            }
            if (saveChange) SaveChangeAndClearCache();
        }

        public virtual void SaveSetting<T, TPropType>(T settings, Expression<Func<T, TPropType>> keySelector, 
            int storeId = 0, bool saveChange = true) where T : ISettings, new()
        {
            var key = settings.GetSettingKey(keySelector);
            var member = keySelector.Body as MemberExpression;
            var propInfo = member.Member as PropertyInfo;
            object value = propInfo.GetValue(settings);
            if (value != null)
                SetSetting(key, value, storeId, saveChange);
            else SetSetting(key, string.Empty, storeId, saveChange);
        }

        public virtual void DeleteSetting<T>(bool saveChange = true) where T : ISettings, new()
        {
            var settingsToDelete = new List<Setting>();
            var allSettings = GetAllSettings();
            var typeOfT = typeof(T);
            foreach(var prop in typeOfT.GetProperties())
                if(prop.CanRead && prop.CanWrite)
                {
                    string key = (typeOfT.Name + "." + prop.Name).ToLowerInvariant();
                    settingsToDelete.AddRange(allSettings.Where(p => p.Name == key));
                }
            _repository.Delete(settingsToDelete);
            if (saveChange) SaveChangeAndClearCache();
        }

        /// <summary>
        /// Xóa setting tại 1 property cho trước
        /// </summary>
        public virtual void DeleteSetting<T, TPropType>(T settings, Expression<Func<T, TPropType>> keySelector,
            int storeId = 0, bool saveChange = true) where T : ISettings, new()
        {
            var key = settings.GetSettingKey(keySelector);
            var setting = GetSetting(key, storeId, false);
            if(setting != null)
            {
                _repository.Delete(setting);
                if (saveChange)
                {
                    _unitOfWork.SaveChanges();
                    _eventPublisher.EntityDeleted(setting);
                }
            }
        }

        /// <summary>
        /// Hàm gọi unitOfWork.SaveChange và phát sự kiện update setting ở mức rộng nhất
        /// </summary>
        public virtual void SaveChangeAndClearCache()
        {
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityAllChange((Setting)null);
        }

        #endregion
    }
}