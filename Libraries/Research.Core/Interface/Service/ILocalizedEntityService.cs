using System;
using System.Linq.Expressions;
using Research.Core;
using Research.Core.Domain.Localization;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// service phụ trách việc đa ngôn ngữ cho 1 số property của các thực thể loại ILocalizedEntity như Product, Category ..v..v..
    /// </summary>
    public partial interface ILocalizedEntityService
    {
        /// <summary>
        /// Lấy về tất cả các localized properties của đối tượng entity. Ko cache
        /// Ghi chú: Hàm này nên hạn chế dùng, chỉ dùng trong các thao tác delete các đối tượng entity
        /// </summary>
        IList<LocalizedProperty> GetLocalizedProperties<T>(T entity)
             where T : BaseEntity, ILocalizedEntity;

        void Delete(LocalizedProperty entity);

        LocalizedProperty GetById(int id);

        /// <summary>
        /// Cho phép lấy về chuỗi dịch theo bộ 4 ( id ngôn ngữ, id thực thể, loại thực thể, tên property )
        /// Kết quả này sẽ được lấy từ cache ra, có thể là thông qua cache tất cả hoặc cache riêng từng bộ khóa tùy câu hình
        /// </summary>
        string GetLocalizedValue(int languageId, int entityId, string localeKeyGroup, string localeKey);

        void Insert(LocalizedProperty entity);

        void Update(LocalizedProperty entity);

        /// <summary>
        /// Lưu chuỗi localeValue xuống database như là chuỗi ngôn ngữ cho property đc chỉ ra bởi keySelector của thực thể entity
        /// và ngôn ngữ languageId
        /// </summary>
        void SaveLocalizedValue<T>(T entity, Expression<Func<T, string>> keySelector, string localeValue,
            int languageId) where T : BaseEntity, ILocalizedEntity;

        /// <summary>
        /// Phiên bản nâng cao của hàm bên trên, khi mà localeValue có thể là 1 kiểu bất kỳ chứ ko bắt buộc là chuỗi ?
        /// </summary>
        void SaveLocalizedValue<T, TPropType>(T entity, Expression<Func<T, TPropType>> keySelector, TPropType localeValue,
            int languageId) where T : BaseEntity, ILocalizedEntity;
    }
}