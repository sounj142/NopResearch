
namespace Research.Core.Configuration
{
    /// <summary>
    /// Có 1 số đối tượng cấu hình đặc biệt cài đặt interface này. Dữ liệu của đối tượng loại này sẽ đc lấy từ bảng Setting trong database
    /// với qui cách là cột Name=[Tên class].[Tên Property]. Việc quản lý dữ liệu loại này sẽ được thực hiện trong ISettingService
    /// </summary>
    public interface ISettings
    {
    }
}
