
namespace Research.Core.Domain.Seo
{
    /// <summary>
    /// Dùng 1 đối tượng riêng lưu trong cache nhằm tránh việc sử dụng nhầm dữ liệu trong cache như 1 thực thể UrlRecord bình thường
    /// </summary>
    public class UrlRecordForCaching
    {
        public int Id { get; set; }

        public int EntityId { get; set; }

        public string EntityName { get; set; }

        public string Slug { get; set; }

        public bool IsActive { get; set; }

        public int LanguageId { get; set; }

        public static UrlRecord Transform(UrlRecordForCaching obj)
        {
            if (obj == null) return null;
            return new UrlRecord
            {
                Id = obj.Id,
                EntityId = obj.EntityId,
                EntityName = obj.EntityName,
                IsActive = obj.IsActive,
                LanguageId = obj.LanguageId,
                Slug = obj.Slug
            };
        }

        public static UrlRecordForCaching Transform(UrlRecord obj)
        {
            if (obj == null) return null;
            return new UrlRecordForCaching
            {
                Id = obj.Id,
                EntityId = obj.EntityId,
                EntityName = obj.EntityName,
                IsActive = obj.IsActive,
                LanguageId = obj.LanguageId,
                Slug = obj.Slug
            };
        }
    }
}