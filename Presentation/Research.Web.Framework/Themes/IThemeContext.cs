
namespace Research.Web.Framework.Themes
{
    /// <summary>
    /// Đối tượng themeContext phải cài đặt interface này. Cho phép cấu hình tên của theme hiện hành ( ứng dụng có thể có nhiều theme,
    /// nhưng sẽ có 1 cái là hiện hành, cấu hình đc )
    /// 
    /// Thêm hiện hành là tùy store, tùy người dùng cụ thể ( có thể cấu hình ). Vòng đời của IThemeContext và per request
    /// </summary>
    public interface IThemeContext
    {
        /// <summary>
        /// Lấy hoặc thiết lập tên theme hiện hành của request hiện tại. Tên theme là tên thư mục con chứa theme ?
        /// </summary>
        string WorkingThemeName { get; set; }
    }
}
