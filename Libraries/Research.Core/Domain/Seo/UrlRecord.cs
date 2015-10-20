

using System;
namespace Research.Core.Domain.Seo
{
    /// <summary>
    /// Đối tượng chứa dữ liệu là 1 bảng ánh xạ ( id - loại entity - language ) --- ( slug string ), cho phép hỗ
    /// trợ link đa ngôn ngữ 1 phân đoạn với GenericUrlRoute
    /// 
    /// Điểm đặc biệt là UrlRecord ko có khóa ngoại trực tiếp đến bảng Language, hơn nữa nó còn "vi phạm qui tắc" bằng cách định nghĩa
    /// languageId=0 ( vốn ko tồn tại trong bảng language ) làm ngôn ngữ mặc định, đồng thời duy trì 1 bộ dữ liệu UrlRecord đầy đủ nhất
    /// cho languageId = 0 để dùng làm Url mặc định( và dữ liệu này sẽ được ưu tiên luôn luôn có ), 
    /// trong khi Url cho các ngôn ngữ khác với languageId thực ( = 1, 2..... ) sẽ chỉ có khi được khai báo cụ thể
    /// </summary>
    public partial class UrlRecord : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the slug
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the record is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Ngày bị unactive gần nhất
        /// </summary>
        public DateTime? LastUnactive { get; set; }
    }
}
