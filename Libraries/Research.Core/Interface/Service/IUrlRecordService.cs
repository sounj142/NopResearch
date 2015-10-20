using Research.Core;
using Research.Core.Domain.Seo;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Sẽ cho phép cache theo 1 trong 2 cơ chế : Cache tất cả các slug hoặc cache theo từng key chỉ khi có nhu cầu sử dụng
    /// 
    /// Chúng ta sẽ cache thẳng đối tượng UrlRecord vào trong cache, nhưng sẽ clone lại dữ liệu 1 lần để đảm bảo
    /// ko còn dây mơ rễ má với EF
    /// 
    /// Hệ thống sẽ cố đảm bảo để cột Slug là unique, bất kể dữ liệu là active hay unactive, nếu có trùng xảy ra, truy vấn
    /// có thể bị định hướng sai, vì các url cả active và unactive đều tham gia vào điều hướng theo các cách khác nhau
    /// </summary>
    public partial interface IUrlRecordService
    {
        void Delete(UrlRecord entity);

        UrlRecord GetById(int id);

        void Insert(UrlRecord entity);

        void Update(UrlRecord entity);

        /// <summary>
        /// Lấy UrlRecord theo khóa là chuỗi slug, dữ liệu được đọc trực tiếp từ database 
        /// </summary>
        UrlRecord GetBySlug(string slug);

        /// <summary>
        /// Lấy ra UrlRecord từ trong cache. Để tăng hiệu năng, đối tượng trả về chính là đối tượng ở trong cache
        /// ko hề qua thao tác clone, nên ko được phép sửa chữa gì trên đối tượng này mà chỉ có thể đọc mà thôi
        /// </summary>
        UrlRecordForCaching GetBySlugCached(string slug);

        /// <summary>
        /// Hàm tìm kiếm urlRecord từ database
        /// </summary>
        IPagedList<UrlRecord> GetAllUrlRecords(string slug, int pageIndex, int pageSize);

        /// <summary>
        /// Tìm slug từ cache với bộ 3 khóa cho trước
        /// </summary>
        string GetActiveSlugCached(int entityId, string entityName, int languageId);

        /// <summary>
        /// Chịu trách nhiệm lưu chuỗi slug cho đối tượng entity, trong đó entity là ISlugSupported ( có thể là
        /// Product, Category, Manufacture .... ). Tùy theo slug đó đã tồn tại hay chưa mà thao tác sẽ là cập nhật
        /// hoặc là ghi mới
        /// </summary>
        UrlRecord SaveSlug<T>(T entity, string slug, int languageId, bool autoAddTailDigit = false)
            where T : BaseEntity, ISlugSupported;
    }
}
