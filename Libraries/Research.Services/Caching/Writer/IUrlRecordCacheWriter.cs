using System;
using Research.Core.Domain.Seo;
using Research.Services.Caching.Models;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface IUrlRecordCacheWriter
    {
        /// <summary>
        /// Cache tất cả các UrlRecord có trong database bất kể url đó là active hay unactive
        /// </summary>
        UrlRecordCachePackage GetAll(Func<UrlRecordCachePackage> acquire);

        /// <summary>
        /// Cho phép lấy về UrlRecord tương ứng với chuỗi slug cho trước, kết quả trả về có thể là active hoặc unactive
        /// </summary>
        UrlRecordForCaching GetBySlug(string slug, Func<UrlRecordForCaching> acquire);

        /// <summary>
        /// Trả về 1 slug tương ứng với bộ 3 khóa cho trước, nhưng chỉ trả về khi UrlRecord đó là ACTIVE, trả vể chuỗi rỗng
        /// trong các trường hợp còn lại
        /// </summary>
        string GetByKeys(int entityId, string entityName, int languageId,
            Func<string> acquire);

        bool TryGetByKeys(int entityId, string entityName, int languageId,
            out string result);
    }
}
