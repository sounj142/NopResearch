using System;

namespace Research.Core.Domain.Tasks
{
    /// <summary>
    /// Các Task thực thi bất đồng bộ sẽ được cấu hình từ database, sẽ có 1 bảng ScheduleTask đảm nhận việc này. Như thế, ta có thể cấu
    /// hình chạy các tác vụ cần thiết như gửi email, clear cache, clear log, delete guest, ... 1 cách động theo nhu cầu
    /// </summary>
    public class ScheduleTask: BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the run period (in seconds)
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Gets or sets the type of appropriate ITask class
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether a task is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether a task should be stopped on some error
        /// </summary>
        public bool StopOnError { get; set; }

        public DateTime? LastStartUtc { get; set; }

        public DateTime? LastEndUtc { get; set; }

        public DateTime? LastSuccessUtc { get; set; }

        public bool RunWithAnotherTask { get; set; }
    }
}
