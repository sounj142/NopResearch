
namespace Research.Core.Infrastructure
{

    /// <summary>
    /// Những công việc muốn chạy vào thời điểm app start sẽ chỉ cần cài đặt bởi giao diện này, và có 1 hàm tạo ko tham số là đủ
    /// 
    /// Đến thời điểm AppStart, IEngine sẽ tự động tìm tất cả các class cài đặt IStartupTask và gọi đến Execute() theo thứ tự Order
    /// ( gọi lần lượt chứ ko phải bất đồng bộ )
    /// </summary>
    public interface IStartupTask
    {
        /// <summary>
        /// Hàm thực thi của Task
        /// </summary>
        void Execute();

        /// <summary>
        /// Thứ tự thực thi
        /// </summary>
        int Order { get; }
    }
}
