using System.Collections.Generic;
using System.Threading.Tasks;

namespace Research.Core.Plugins
{
    /// <summary>
    /// Official feed manager (official plugins from www.nopCommerce.com site)
    /// 
    /// 
    /// Nói chung đây là interface cho phép tìm kiếm các phiên bản plugin trên máy chủ Nop xem có những phiên bản nào,
    /// để dẫn chúng ta đến trang mua và down về. Các phiên bản này có thể là thu phí hoặc là miễn phí
    /// 
    /// Ngoại trừ chức năng tìm các plugin trên máy chủ, láy về danh sách tên, hình ảnh, giá cả, mô tả và dẫn link đến trang mua, nó
    /// ko làm cái gì ra hồn cả
    /// 
    /// Toàn bộ những class, interface thuộc họ *OfficialFeed* đều chỉ xài cho việc lấy danh sách plugin trên server Nop về xem
    /// chứ ko làm bất cứ thứ gì ra hồn cả
    /// </summary>
    public interface IOfficialFeedManager
    {
        /// <summary>
        /// Get categories
        /// Lấy về thông qua giao thức http tất cả các danh mục plugin hiện có trên máy chủ Nop
        /// </summary>
        Task<IList<OfficialFeedCategory>> GetCategories();

        /// <summary>
        /// Get versions
        /// Lấy về thông qua giao thức http tất cả các phiên bản hiện có trên máy chủ Nop
        /// </summary>
        Task<IList<OfficialFeedVersion>> GetVersions();

        /// <summary>
        /// Kết nối với máy chủ Nop để lấy về danh sách các plugin theo các thông số và phân trang tương ứng
        /// Get all plugins
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="versionId">Version identifier</param>
        /// <param name="price">Price; 0 - all, 10 - free, 20 - paid</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Plugins</returns>
        Task<IPagedList<OfficialFeedPlugin>> GetAllPlugins(int categoryId = 0, int versionId = 0, int price = 0,
            string searchTerm = "", int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
