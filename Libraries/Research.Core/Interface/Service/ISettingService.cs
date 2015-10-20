using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Research.Core.Configuration;
using Research.Core.Domain.Configuration;

namespace Research.Core.Interface.Service
{
    public partial interface ISettingService
    {
        void DeleteSetting(Setting setting, bool saveChange = true);

        /// <summary>
        /// Get setting by key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all stores) value should be loaded if a value specific for a certain is not found</param>
        /// <returns>Setting</returns>
        Setting GetSetting(string key, int storeId = 0, bool loadSharedValueIfNotFound = false, bool getFromStaticCache = true);

        /// <summary>
        /// Lấy về giá trị setting với key cho trước. 
        /// </summary>
        /// <typeparam name="T">Kiểu sẽ được ép về</typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue">default value nếu ko tìm thấy</param>
        /// <param name="storeId"></param>
        /// <param name="loadSharedValueIfNotFound">Có lấy giá trị mặc đinh ( storeId = 0) nếu ko tìm thấy hay ko</param>
        /// <returns></returns>
        T GetSettingByKey<T>(string key, T defaultValue = default(T), int storeId = 0, bool loadSharedValueIfNotFound = false);

        /// <summary>
        /// Thiết lập giá trị value cho khóa key với các tham số đc chỉ rõ
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="saveChange">A value indicating whether to clear cache after setting update</param>
        void SetSetting<T>(string key, T value, int storeId = 0, bool saveChange = true);

        /// <summary>
        /// Kiểm tra xem 1 property của đối ISetting là có trong bảng Setting trong Database hay ko. VD ta có thể kiểm tra
        /// LocalizationSettings.UseImagesForLanguageSelection xem nó có trong database hay ko
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>true -setting exists; false - does not exist</returns>
        bool SettingExists<T, TPropType>(T settings, Expression<Func<T, TPropType>> keySelector, int storeId)
            where T : ISettings, new();

        /// <summary>
        /// Hàm cho phép load động 1 đối tượng loại ISettings từ data của bảng Setting với qui cách Name=[Tên class].[Tên property]
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="storeId">Store identifier for which settigns should be loaded</param>
        T LoadSetting<T>(int storeId = 0) where T : ISettings, new();

        object LoadSetting(Type type, int storeId = 0);

        /// <summary>
        /// Lưu toàn bộ thông tin từ đối tượng T xuống bảng Setting trong database
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="storeId">Store identifier</param>
        /// <param name="settings">Setting instance</param>
        void SaveSetting<T>(T settings, int storeId = 0, bool saveChange = true) where T : ISettings, new();

        /// <summary>
        /// Lưu 1 property của đối tượng settings xuống database
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="storeId">Store ID</param>
        /// <param name="saveChange">A value indicating whether to clear cache after setting update</param>
        void SaveSetting<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector,
            int storeId = 0, bool saveChange = true) where T : ISettings, new();

        /// <summary>
        /// Xóa bỏ tất cả các settig có kiểu T khỏi bảng Setting
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        void DeleteSetting<T>(bool saveChange = true) where T : ISettings, new();

        /// <summary>
        /// Xóa bỏ 1 property
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="storeId">Store ID</param>
        void DeleteSetting<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector, int storeId = 0, bool saveChange = true) where T : ISettings, new();

        /// <summary>
        /// Savechange và Clear cache. Cơ chế cache của setting sẽ ko chỉ chứa get by name, get all mà còn chứa cả những đối tượng ISetting rời rạc
        /// nữa, sẽ khá phức tạp khi clear ?
        /// </summary>
        void SaveChangeAndClearCache();

        /// <summary>
        /// Lấy tất cả setting từ cache
        /// </summary>
        IList<Setting> GetAllSettings();

        /// <summary>
        /// Lấy Setting từ cache hoặc đọc trực tiếp database
        /// </summary>
        Setting GetById(int id, bool getFromStaticCache);

        void UpdateSetting(Setting setting, bool saveChange = true);
    }
}
