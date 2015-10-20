using System;
using System.Configuration;
using System.Xml;
using Research.Core.Infrastructure;

namespace Research.Core.Configuration
{
    /// <summary>
    /// Đối tượng chưa thông tin cấu hình ứng dụng, cài đặt giao diện IConfigurationSectionHandler để có thể trực tiếp ép kiểu 
    /// từ kết quả trả về của ConfigurationManager.GetSection("NopConfig")
    /// </summary>
    public partial class NopConfig : IConfigurationSectionHandler
    {
        /// <summary>
        /// cấu hình kiểm tra và load assemblies từ thư mục bin
        /// </summary>
        public bool DynamicDiscovery { get; private set; }

        /// <summary>
        /// 1 cài đặt custom của <see cref="IEngine"/> để quản lý ứng dụng thay cho mặc định là NopEngine
        /// </summary>
        public string EngineType { get; private set; }
        /// <summary>
        /// Chỉ định rõ thư mục lưu trữ theme (~/Themes/)
        /// </summary>
        public string ThemeBasePath { get; private set; }
        /// <summary>
        ///  có bỏ qua ko chạy những startup task hay ko
        /// </summary>
        public bool IgnoreStartupTasks { get; private set; }
        /// <summary>
        /// Path to database with user agent strings
        /// </summary>
        public string UserAgentStringsPath { get; private set; }

        /// <summary>
        /// Creates a configuration section handler. Đọc nội dung xml để lấy về các thông tin cấu hình cần thiết cho toàn bộ ứng dụng
        /// </summary>
        /// <param name="parent">Parent object.</param>
        /// <param name="configContext">Configuration context object.</param>
        /// <param name="section">Section XML node.</param>
        /// <returns>The created section handler object.</returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            var config = new NopConfig();
            // dạng : <DynamicDiscovery Enabled="true" />
            ReadXmlConfigData(section, "DynamicDiscovery", "Enabled", value =>
            { config.DynamicDiscovery = Convert.ToBoolean(value); });
            // dạng : <Engine Type="Nop.MyEngine" />
            ReadXmlConfigData(section, "Engine", "Type", value =>
            { config.EngineType = value; });

            ReadXmlConfigData(section, "Startup", "IgnoreStartupTasks", value =>
            { config.IgnoreStartupTasks = Convert.ToBoolean(value); });

            ReadXmlConfigData(section, "Themes", "basePath", value =>
            { config.ThemeBasePath = value; });

            ReadXmlConfigData(section, "UserAgentStrings", "databasePath", value =>
            { config.UserAgentStringsPath = value; });

            return config;
        }

        protected void ReadXmlConfigData(XmlNode section, string nodeName, string attributeName, Action<string> method)
        {
            var node = section.SelectSingleNode(nodeName);
            if (node != null && node.Attributes != null)
            {
                var attribute = node.Attributes[attributeName];
                if (attribute != null)
                    method(attribute.Value);
            }
        }
    }
}
