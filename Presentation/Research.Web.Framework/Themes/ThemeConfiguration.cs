using System.Xml;

namespace Research.Web.Framework.Themes
{
    /// <summary>
    /// Lớp đọc Thông tin cấu hình cho theme. Thông tin theme thường đặt trong thư mục của theme, VD : ~/Themes/DefaultClean/theme.config
    /// </summary>
    public class ThemeConfiguration
    {
        public XmlNode ConfigurationNode { get; protected set; }

        public string Path { get; protected set; }

        public string PreviewImageUrl { get; protected set; }

        public string PreviewText { get; protected set; }

        public bool SupportRtl { get; protected set; }

        public string ThemeName { get; protected set; }

        public string ThemeTitle { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="themeName">Thường là tên thư mục chứa theme con</param>
        /// <param name="path">Thường là đường dẫn thư mục theme</param>
        /// <param name="doc">Đối tượng xml đọc file theme.config</param>
        public ThemeConfiguration(string themeName, string path, XmlDocument doc)
        {
            this.ThemeName = themeName;
            this.Path = path;
            var node = doc.SelectSingleNode("Theme");
            if(node != null)
            {
                this.ConfigurationNode = node;
                var attribute = node.Attributes["title"];
                this.ThemeTitle = attribute == null ? string.Empty : attribute.Value;
                attribute = node.Attributes["supportRTL"];
                this.SupportRtl = attribute == null ? false : bool.Parse( attribute.Value);
                attribute = node.Attributes["previewImageUrl"];
                this.PreviewImageUrl = attribute == null ? string.Empty : attribute.Value;
                attribute = node.Attributes["previewText"];
                this.PreviewText = attribute == null ? string.Empty : attribute.Value;
            }
        }
    }
}
