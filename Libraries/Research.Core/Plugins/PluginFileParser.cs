using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Research.Core.Plugins
{
    /// <summary>
    /// Plugin files parser
    /// Lớp chịu trách nhiệm đọc các file cấu hình của plugin, để từ đó tạo ra đối tượng PluginDescriptor tương ứng. 
    /// Cũng như ASP.net qui định Web.config là file cấu hình, hệ thống Nop qui định các file sau là những file cấu hình của plugin
    /// và bắt buộc phải có để plugin có thể hoạt động được
    /// + File Description.txt ở thư mục gốc project plugin
    /// </summary>
    public static class PluginFileParser
    {
        /// <summary>
        /// Hàm đọc 1 file với đường dẫn filePath và trả về 1 list các dòng khác rỗng có trong file đó
        /// </summary>
        public static IList<string> ParseInstalledPluginsFile(string filePath)
        {
            var lines = new List<string>();
            if (!File.Exists(filePath)) return lines;
            var text = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(text)) return lines;

            //Old way of file reading. This leads to unexpected behavior when a user's FTP program transfers these files as ASCII (\r\n becomes \n).
            //var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            using(var reader = new StringReader(text))
            {
                string str;
                while((str = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(str))
                        lines.Add(str.Trim());
                }
            }
            return lines;
        }

        /// <summary>
        /// Hàm ghi nội dung các dòng cho bởi pluginSystemNames xuống file filePath, thường dùng để cập nhật lại các file cấu hình plugin
        /// </summary>
        public static void SaveInstalledPluginsFile(IList<string> pluginSystemNames, string filePath)
        {
            if (pluginSystemNames == null) throw new ArgumentNullException("pluginSystemNames");

            var builder = new StringBuilder();
            foreach (var line in pluginSystemNames)
                builder.AppendLine(line);
            File.WriteAllText(filePath, builder.ToString());
        }

        /// <summary>
        /// Hàm đọc file cấu hình từ đường dẫn filePath, trả về kết quả là đối tượng PluginDescriptor miêu tả cấu hình của plugin.
        /// Thường thì sẽ đọc nội dung file Description.txt ở thư mục gốc plugin
        /// </summary>
        public static PluginDescriptor ParsePluginDescriptionFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("filePath must not empty");

            var descriptor = new PluginDescriptor();
            if (!File.Exists(filePath)) throw new Exception("File " + filePath + " not exists");
            var text = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(text)) return descriptor;

            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        str = str.Trim();
                        int index = str.IndexOf(':');
                        if(index > 0)
                        {
                            string key = str.Substring(0, index).Trim();
                            string value = str.Substring(index + 1).Trim();
                            // chỉ xử lý khi key và value đều khác rỗng ( cái này khác code nguyên bản Nop )
                            if(!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                            {
                                switch (key)
                                {
                                    case "Group":
                                        descriptor.Group = value;
                                        break;
                                    case "FriendlyName":
                                        descriptor.FriendlyName = value;
                                        break;
                                    case "SystemName":
                                        descriptor.SystemName = value;
                                        break;
                                    case "Version":
                                        descriptor.Version = value;
                                        break;
                                    case "SupportedVersions":
                                        descriptor.SupportedVersions = value
                                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(p => p.Trim())
                                            .Where(p => !string.IsNullOrEmpty(p))
                                            .ToList();
                                        break;
                                    case "Author":
                                        descriptor.Author = value;
                                        break;
                                    case "DisplayOrder":
                                        int displayOrder;
                                        if (int.TryParse(value, out displayOrder))
                                            descriptor.DisplayOrder = displayOrder;
                                        break;
                                    case "FileName":
                                        descriptor.PluginFileName = value;
                                        break;
                                    case "LimitedToStores":
                                        int storeId;
                                        foreach(var s in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)))
                                        {
                                            if (int.TryParse(s, out storeId))
                                                descriptor.LimitedToStores.Add(storeId);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            // các phiên bản nop dưới 2.00 ko có tham số "SupportedVersions", nên nếu trong file cấu hình ko chỉ rõ SupportedVersions
            // là những phiên bản nào, mặc nhiên ta qui định là từ 2.00 trở lên
            if (descriptor.SupportedVersions.Count == 0)
                descriptor.SupportedVersions.Add("2.00");
            return descriptor;
        }

        /// <summary>
        /// Ghi nội dung cấu hình plugin xuống file Description.txt trong thư mục gốc của plugin. Tức là thế này : Plugin sau khi biên dich
        /// sẽ được đặt ở 1 thư mục cụ thể trong ứng dụng Nop theo qui định. Thông qua plugin.OriginalAssemblyFile, ta sẽ lấy được đường
        /// dẫn đến file dll của plugin, từ đường dẫn đó, ta sẽ lấy được đường dẫn của file cấu hình Description.txt ( được đặt cùng
        /// thư mục với file plugin.OriginalAssemblyFile )
        /// </summary>
        public static void SavePluginDescriptionFile(PluginDescriptor plugin)
        {
            if (plugin == null) throw new ArgumentNullException("plugin");

            // lấy về đường dẫn vật lý của file Description.txt
            if (plugin.OriginalAssemblyFile == null)
                throw new Exception(string.Format("Cannot load original assembly path for {0} plugin.", plugin.SystemName));
            string filePath = Path.Combine(plugin.OriginalAssemblyFile.Directory.FullName, "Description.txt");
            if (!File.Exists(filePath))
                throw new Exception(string.Format("Description file for {0} plugin does not exist. {1}", plugin.SystemName, filePath));

            string formatString = "{0}: {1}";
            var lines = new List<string>();
            lines.Add(string.Format(formatString, "Group", plugin.Group));
            lines.Add(string.Format(formatString, "FriendlyName", plugin.FriendlyName));
            lines.Add(string.Format(formatString, "SystemName", plugin.SystemName));
            lines.Add(string.Format(formatString, "Version", plugin.Version));
            if (plugin.SupportedVersions.Count > 0)
                lines.Add(string.Format(formatString, "SupportedVersions", string.Join(",",  plugin.SupportedVersions)));
            lines.Add(string.Format(formatString, "Author", plugin.Author));
            lines.Add(string.Format(formatString, "DisplayOrder", plugin.DisplayOrder));
            lines.Add(string.Format(formatString, "FileName", plugin.PluginFileName));
            if (plugin.LimitedToStores.Count > 0)
                lines.Add(string.Format(formatString, "LimitedToStores", string.Join(",", plugin.LimitedToStores)));

            SaveInstalledPluginsFile(lines, filePath);
        }
    }
}
