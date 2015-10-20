using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Localization;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Service quản lý các chuỗi resource trong hệ thống
    /// </summary>
    public partial interface ILocalizationService
    {
        void DeleteLocaleStringResource(LocaleStringResource entity, bool saveChange = true);

        LocaleStringResource GetById(int id);

        LocaleStringResource GetLocaleStringResourceByName(string resourceName);

        LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId, bool logIfNotFound = true);

        IQueryable<LocaleStringResource> GetAllQueryable(int languageId);

        IList<LocaleStringResource> GetAll(int languageId);

        /// <summary>
        /// Hàm cho phép lấy ra tất cả các chuỗi resource theo ngôn ngữ languageId và cache tất cả vào 1 key static cache dưới dạng Dictionary
        /// Hàm này sẽ được gọi lại bởi GetResource(), ko được gọi hàm 1 cách trực tiếp vì cơ chế cache có thể có 2 khả năng:
        /// cache tất cả/cache từng key. Phơi bày phương thức này ra và để nó bị bên ngoài gọi có thể khiến tình trạng
        /// cả cache tất cả và cache từng key cùng tồn tại
        /// 
        /// Sửa: Đã ẩn nó khỏi interface để ngăn bên ngoài tự ý gọi vào, để GetResource tự mình kiểm tra và sử dụng cơ chế
        /// cache phù hợp với cấu hình
        /// </summary>
        // IDictionary<string, KeyValuePair<int, string>> GetAllResourceValues(int languageId);

        void InsertLocaleStringResource(LocaleStringResource entity, bool saveChange = true);

        void UpdateLocaleStringResource(LocaleStringResource entity, bool saveChange = true);

        /// <summary>
        /// Hàm lấy ra chuỗi resouce với khóa cho trước. Bản thân hàm sẽ tự quyết định cache theo cách nào dựa vào thông tin cấu hình website
        /// Có thể là cache static toàn bộ hay static từng key. Đây là điểm bên ngoài gọi vào để lấy ra resource được cache.
        /// Nếu ko tìm thấy thì sẽ trả về khóa resourceKey
        /// </summary>
        string GetResource(string resourceKey, bool logIfNotFound = true);

        string GetResource(string resourceKey, int languageId, bool logIfNotFound = true,
            string defaultValue = "", bool returnEmptyIfNotFound = false);

        string ExportResourcesToXml(Language language);

        void ImportResourcesFromXml(Language language, string xml);

        /// <summary>
        /// Save change những thay đổi xuống database và phát sinh sự kiện mức allchanged
        /// </summary>
        void SaveChange();
    }
}
