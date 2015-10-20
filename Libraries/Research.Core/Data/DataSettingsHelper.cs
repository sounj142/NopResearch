
namespace Research.Core.Data
{
    /// <summary>
    /// Chả hiểu lớp này để làm gì nữa, toàn làm việc thừa thải
    /// 
    /// Lớp chứa 1 cờ bool static cho biết database đã được install hay chưa. Nó sẽ khởi tạo việc
    /// install 1 lần duy nhất nếu database chưa được install, và cũng cho phép thiết lập lại cờ này thông qua ResetCache
    /// </summary>
    public partial class DataSettingsHelper
    {
        private static bool? _databaseIsInstalled;
        private static object _lockObj = new object();
        /// <summary>
        /// Kiểm tra xem database đã được khởi tạo chưa bằng cách đọc vào file setting.txt. Quá trình đọc này chỉ được thực hiện duy nhất 1 lần
        /// bởi kết quả sẽ được cache vào 1 biến static
        /// </summary>
        /// <returns></returns>
        public static bool DatabaseIsInstalled()
        {
            if (!_databaseIsInstalled.HasValue) //// khác chỗ này ( thiết lập lock để ngăn truy cập đồng thời ? )
            {
                lock(_lockObj)
                {
                    if (_databaseIsInstalled == null)
                    {
                        var manager = new DataSettingsManager();
                        var setting = manager.LoadSettings();
                        _databaseIsInstalled = setting != null && setting.IsValid(); //// khác ở đây
                    }
                }
            }
            return _databaseIsInstalled.Value;
        }

        /// <summary>
        /// Hàm cho phép thiết lập giá trị static về null để chạy lại việc kiểm tra file setting.text
        /// Đây có thể coi là mô hình cache - sử dung - clear cache với giá trị cache được lưu vào 1 biến static
        /// </summary>
        public static void ResetCache()
        {
            _databaseIsInstalled = null;
        }
    }
}
