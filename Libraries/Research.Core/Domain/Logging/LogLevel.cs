

namespace Research.Core.Domain.Logging
{
    /// <summary>
    /// Miêu tả cho loại log. Mỗi thông tin log được lưu xuống sẽ được phân loại thành 1 trong các loại này
    /// Việc dùng enum giúp giảm bớt 1 bảng LogType ko cần thiết
    /// </summary>
    public enum LogLevel
    {
        Debug = 10,
        Information = 20,
        Warning = 30,
        Error = 40,
        Fatal = 50
    }
}
