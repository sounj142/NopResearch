using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Seo;

namespace Research.Services.Caching.Models
{
    /// <summary>
    /// Đối tượng định nghĩa 2 List dùng để tra cứu các UrlRecord theo 2 bộ key, dùng trong trường hợp mà hệ thống
    /// cấu hình cache toàn bộ các UrlRecord 
    /// </summary>
    public class UrlRecordCachePackage
    {
        /// <summary>
        /// Lưu tất cả các url record bát kể active hay unactive, sắp xếp tăng dần theo slug và sau đó giảm dần theo IsActive ( dù rằng
        /// thực tế nếu url recỏd đảm bảo slug là unique thì sẽ ko cần giảm dần theo IsActive, điều này chỉ để bảo đảm vẫn giải quyết đc
        /// khi dữ liệu ko đúng )
        /// </summary>
        public IList<UrlRecordForCaching> ListBySlug { get; private set; }

        /// <summary>
        /// Do nhu cầu truy vấn theo 3 khóa chỉ lấy những trường active, list này sẽ chỉ tập trung lưu giữ những trường nào là active
        /// </summary>
        public IList<UrlRecordForCaching> ListByThreeKeys { get; private set; }

        /// <summary>
        /// Hàm đọc vào dữ liệu UrlRecord từ database để tổ chức dữ liệu cho 2 List
        /// </summary>
        public void Initialize(IQueryable<UrlRecord> query)
        {
            // sắp xếp sẵn theo Slug để tiện cho tìm kiếm nhị phân theo slug
            query = query.OrderBy(p => p.Slug).ThenBy(p => p.IsActive).ThenBy(p => p.Id);
            ListBySlug = new List<UrlRecordForCaching>();

            foreach (var urlRecord in query)
            {
                var cacheObj = UrlRecordForCaching.Transform(urlRecord);
                ListBySlug.Add(cacheObj);
            }

            // tổ chức dữ liệu cho ListByThreeKeys từ ListBySlug
            ListByThreeKeys = ListBySlug.Where(p => p.IsActive)
                .OrderBy(p => p.EntityId)
                .ThenBy(p => p.LanguageId)
                .ThenBy(p => p.EntityName, StringComparer.InvariantCulture)
                .ThenBy(p => p.Id).ToList();
        }

        /// <summary>
        /// Tìm kiếm nhị phân UrlRecord tương ứng với chuỗi slug, trong trường hợp dữ liệu sai và có nhiều hơn 1 kết quả thì ưu tiên
        /// lấy trường active có id lớn nhất
        /// </summary>
        public UrlRecordForCaching Search(string slug)
        {
            int begin = 0, end = ListBySlug.Count - 1, middle, next, compare;
            while(begin <= end)
            {
                middle = (begin + end) >> 1;
                compare = string.Compare(slug, ListBySlug[middle].Slug, StringComparison.InvariantCultureIgnoreCase);
                if (compare < 0) end = middle - 1;
                else if (compare > 0) begin = middle + 1;
                else
                {
                    // tìm thấy, tiến hành tìm phần tử có chỉ số cao nhất
                    next = middle + 1;
                    while(next < ListBySlug.Count && string.Equals(slug, ListBySlug[next].Slug, StringComparison.InvariantCultureIgnoreCase))
                    {
                        middle = next;
                        next = middle + 1;
                    }
                    return ListBySlug[middle];
                }
            }
            return null;
        }

        public string Search(int entityId, string entityName, int languageId)
        {
            int begin = 0, end = ListByThreeKeys.Count - 1, middle, next, compare;
            while (begin <= end)
            {
                middle = (begin + end) >> 1;
                var current = ListByThreeKeys[middle];
                if (entityId < current.EntityId) end = middle - 1;
                else if (entityId > current.EntityId) begin = middle + 1;
                else
                {
                    if (languageId < current.LanguageId) end = middle - 1;
                    else if (languageId > current.LanguageId) begin = middle + 1;
                    else
                    {
                        compare = string.Compare(entityName, current.EntityName, StringComparison.InvariantCulture);
                        if (compare < 0) end = middle - 1;
                        else if (compare > 0) begin = middle + 1;
                        else
                        {
                            // bộ 3 key là bằng nhau, tiến hành tìm phần tử có Id lớn nhất nếu có
                            // tìm thấy, tiến hành tìm phần tử có chỉ số cao nhất
                            next = middle + 1;
                            while (next < ListByThreeKeys.Count && ListByThreeKeys[next].EntityId == entityId &&
                                ListByThreeKeys[next].LanguageId == languageId &&
                                string.Equals(entityName, ListByThreeKeys[next].EntityName, StringComparison.InvariantCulture))
                            {
                                middle = next;
                                next = middle + 1;
                            }
                            return ListByThreeKeys[middle].Slug ?? string.Empty;
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
