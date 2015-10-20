
namespace Research.Core.Infrastructure
{
    /// <summary>
    /// Những tác vụ nào muốn đc chạy 1 cách tự động bởi hệ thống thì cài đặt giao diện này. Việc khởi chạy ITask được cấu hình từ database
    /// ( bảng ScheduleTask ), trong đó chứa tên đầy đủ của lớp Task, ko phải là load động bằng cách quét tất cả các assembly
    /// </summary>
    public partial interface ITask
    {
        /// <summary>
        /// Hàm là điểm vào của Task
        /// </summary>
        void Execute();
    }
}
