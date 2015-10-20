using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Research.Core;
using Research.Core.Configuration;

namespace Research.Web.Framework.Themes
{
    /// <summary>
    /// Chú ý: Với hàm tạo của mình và bản thân vòng đời per request khi đăng ký với autofact, Ở mỗi request, khi được tạo ra thì
    /// ThemeConfiguration sẽ luôn luôn quét qua danh sách tất cả các thư mục con của thư mục theme hiện hành ( ~/Themes/ ), ở mỗi
    /// thư mục con ấy, nó sẽ tìm file web.config nếu có và đọc file ở dạng xml để lấy lên các thuộc tính cấu hình
    /// 
    /// Nếu đọc cấu hình thành công, thư mục con đó sẽ đc đưa vào danh sách theme
    /// 
    /// Cách làm này cho phép có thể thay đổi động cấu trúc thư mục chứa theme ngay khi ứng dụng đang chạy ( thêm bớt các theme ),
    /// hoặc thay đổi file cấu hình động. Tuy nhiên điều này hiếm khi xảy ra. Cho nên cách làm này tiềm ẩn khả năng giảm hiệu năng 
    /// =>
    /// Cần xem xét khả năng cache static danh sách các theme đang có ( chỉ load 1 lần, và ko cập nhật lại cho đến khi hết thời hạn hoặc clear
    /// cahche ), thêm chức năng clear cache cho danh sách này
    /// </summary>
    public partial class ThemeProvider : IThemeProvider
    {
        #region fields and property and ctors

        public const string ThemeConfigFileName = "theme.config";

        /// <summary>
        /// Chứa danh sách các theme con, bao gồm cả thông tin cấu hình của chúng
        /// </summary>
        private readonly IList<ThemeConfiguration> _themeConfigurations = new List<ThemeConfiguration>();

        /// <summary>
        /// đường dẫn cơ sở của theme, là đường dẫn vật lý, bên trong thư mục này chưa nhiều thư mục con, mỗi thư mục con sẽ là
        /// 1 theme con nếu nó có cấu trúc hợp lệ ( có file theme.config )
        /// </summary>
        private readonly string _basePath;

        public ThemeProvider(NopConfig nopConfig, IWebHelper webHelper)
        {
            _basePath = webHelper.MapPath(nopConfig.ThemeBasePath); // mapPath đường dẫn theme trong config (~/Themes/) thành đường dẫn vật lý
            LoadConfigurations();
        }

        #endregion

        #region method

        public ThemeConfiguration GetThemeConfiguration(string themeName)
        {
            return _themeConfigurations.FirstOrDefault(p => 
                string.Equals(p.ThemeName, themeName, StringComparison.InvariantCultureIgnoreCase));
        }

        public IList<ThemeConfiguration> GetThemeConfigurations()
        {
            return _themeConfigurations;
        }

        public bool ThemeConfigurationExists(string themeName)
        {
            return _themeConfigurations.Any(p =>
                string.Equals(p.ThemeName, themeName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Hàm chịu trách nhiệm từ đường dẫn của thư mục theme gốc, đọc các thư mục con của nó( mỗi thư mục là 1 theme con),
        /// và load file cấu hình tương ứng của các theme con đó
        /// </summary>
        private void LoadConfigurations()
        {
            //TODO:Use IFileStorage?
            foreach(string themePath in Directory.GetDirectories(_basePath))
            {
                var configuration = CreateThemeConfiguration(themePath);
                if (configuration != null) _themeConfigurations.Add(configuration);
            }
        }

        private ThemeConfiguration CreateThemeConfiguration(string themePath)
        {
            var directory = new DirectoryInfo(themePath);
            var configFile = new FileInfo(Path.Combine(themePath, ThemeConfigFileName));
            if(configFile.Exists)
            {
                var doc = new XmlDocument();
                doc.Load(configFile.FullName);
                return new ThemeConfiguration(directory.Name, directory.FullName, doc);
            }
            return null;
        }

        #endregion
    }
}
