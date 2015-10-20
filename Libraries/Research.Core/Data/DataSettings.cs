using System.Collections.Generic;

namespace Research.Core.Data
{
    /// <summary>
    /// Đây là đối tượng cấu hình database, thông tin truy cập CSDL
    /// </summary>
    public partial class DataSettings
    {
        /// <summary>
        /// Provider kết nối
        /// </summary>
        public string DataProvider { get; set; }

        /// <summary>
        /// Chuỗi kết nối
        /// </summary>
        public string DataConnectionString { get; set; }

        /// <summary>
        /// Những cấu hình thêm cho database
        /// </summary>
        public IDictionary<string, string> RawDataSettings { get; private set; }

        public DataSettings()
        {
            RawDataSettings = new Dictionary<string, string>();
        }
        /// <summary>
        /// Thông tin cấu hình hợp lệ chỉ khi 2 property DataProvider, DataConnectionString khác rỗng
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(DataProvider) && !string.IsNullOrEmpty(DataConnectionString);
        }
    }
}
