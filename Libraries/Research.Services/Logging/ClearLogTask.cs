using Research.Core.Infrastructure;
using Research.Core.Interface.Service;

namespace Research.Services.Logging
{
    /// <summary>
    /// 1 tác vụ sẽ clear bảng Log trong database theo định kỳ
    /// Qui trình gọi tác vụ tham khảo các cài đặt Task, TaskThread và TaskManager trong Research.Services.Tasks
    /// 
    /// ClearLogTask sẽ ko tự chạy, để cấu hình các task được chạy, cần vào bảng ScheduleTask trong database
    /// </summary>
    public partial class ClearLogTask : ITask
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Để tạo ra 1 đối tượng ClearLogTask, Autofac sẽ đc sử dụng, ưu tiên dùng TryResolve, nhưng sẽ dùng ResolveUnregistered nếu
        /// TryResolve ko xài đc ( kiểu ko được khai báo DI trong Autofact ). Và nhìn chung, cài đặt hàm tạo của các lơp ITask đủ
        /// đơn giản để ResolveUnregistered có thể giải quyết đc
        /// </summary>
        public ClearLogTask(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Chỉ tập trung Thực hiện công việc của mình. Mọi thứ khác như cấu hình thời gian lặp lại, lên lịch chạy, ...v.v..
        /// đã có nơi khác lo
        /// </summary>
        public void Execute()
        {
            _logger.ClearLog();
        }
    }
}
