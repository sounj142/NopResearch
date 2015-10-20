using Research.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Research.Core.Data
{
    public partial class DataSettingsManager
    {
        protected const char separator = ':';
        /// <summary>
        /// Tên file chứa loại database và chuỗi kết nối database, sẽ được đọc 1 lần vào lúc khởi chạy, nhưng cũng có thể được đọc
        /// lại nếu có yêu cầu
        /// </summary>
        protected const string dataSettingFileName = "Settings.txt";

        /// <summary>
        /// Map 1 virtual path thành đường dẫn vật lý
        /// </summary>
        protected virtual string MapPath(string path)
        {
            return WebCommon.MapPath(path);
        }

        protected virtual DataSettings ParseSettings(string text)
        {
            var shellSettings = new DataSettings();
            if (string.IsNullOrWhiteSpace(text)) return shellSettings;

            var settings = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                    settings.Add(str);
            }

            foreach (var setting in settings)
            {
                var separatorIndex = setting.IndexOf(separator);
                if (separatorIndex == -1)
                {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                switch (key)
                {
                    case "DataProvider":
                        shellSettings.DataProvider = value;
                        break;
                    case "DataConnectionString":
                        shellSettings.DataConnectionString = value;
                        break;
                    default:
                        shellSettings.RawDataSettings.Add(key, value);
                        break;
                }
            }

            return shellSettings;
        }

        /// <summary>
        /// Chuyển dữ liệu settings thành dạng chuỗi để có thể ghi xuống file
        /// 
        /// Trả về chuỗi kết nối được tạo ra từ thông tin lấy từ settings // xem lại mục đích của hàm, nếu là chuỗi kết nối
        /// thì phải tác ra 1 hàm riêng để tạo chuỗi xuất txt
        /// </summary>
        protected virtual string ComposeSettings(DataSettings settings)
        {
            if (settings == null) return string.Empty;

            string result = string.Format("DataProvider: {0}{2}DataConnectionString: {1}{2}",
                                 settings.DataProvider,
                                 settings.DataConnectionString,
                                 Environment.NewLine
                );
            if (settings.RawDataSettings != null && settings.RawDataSettings.Count > 0) //// khác ở đây
            {
                string str = string.Join("", settings.RawDataSettings.Select(p =>
                    string.Format("{0}: {1}{2}", p.Key, p.Value, Environment.NewLine)));
                result += str;
            }

            return result;
        }

        /// <summary>
        /// Đọc thông tin cấu hình database từ 1 đường dẫn cho trước, hoặc đường dẫn mặc định từ ~/App_Data/Settings.txt
        /// </summary>
        public virtual DataSettings LoadSettings(string filePath = null)
        {
            filePath = GetPhysicalFilePath(filePath);
            
            if (File.Exists(filePath))
                return ParseSettings(File.ReadAllText(filePath));
            return new DataSettings();
        }

        /// <summary>
        /// Lưu thông tin cấu hình trong setting xuống file, dùng khi admn muốn thay đổi cấu hình kết nối CSDL
        /// </summary>
        public virtual void SaveSettings(DataSettings settings, string filePath = null)
        {
            if (settings == null) throw new NullReferenceException("settings");
            filePath = GetPhysicalFilePath(filePath);

            if(!File.Exists(filePath))
                using (File.Create(filePath)) { }

            File.WriteAllText(filePath, ComposeSettings(settings));
        }
        
        private string GetPhysicalFilePath(string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
                filePath = Path.Combine(MapPath("~/App_Data/"), dataSettingFileName);
            else if (filePath.StartsWith("~/"))
                filePath = MapPath(filePath);
            return filePath;
        }
    }
}
