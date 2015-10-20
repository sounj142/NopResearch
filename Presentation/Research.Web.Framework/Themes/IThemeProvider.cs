using System.Collections.Generic;

namespace Research.Web.Framework.Themes
{
    /// <summary>
    /// Provider đóng vai trò bộ cung cấp theme. Nó đại diện cho 1 thư mục theme cha chưa nhiều thư mục con ( mỗi cái là 1 theme ).
    /// Chịu trách nhiệm đọc về danh sách các thư mục con, phân tích file config nếu có và trả về thông tin các theme con mà nó nắm giữ
    /// cho bên ngoài
    /// </summary>
    public partial interface IThemeProvider
    {
        /// <summary>
        /// Lấy về theme với tên cho trước ( tên thư mục chứa theme )
        /// </summary>
        ThemeConfiguration GetThemeConfiguration(string themeName);

        /// <summary>
        /// Lấy về tất cả các theme
        /// </summary>
        IList<ThemeConfiguration> GetThemeConfigurations();

        bool ThemeConfigurationExists(string themeName);
    }
}
