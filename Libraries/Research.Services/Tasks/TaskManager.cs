using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Research.Core.Domain.Tasks;
using Research.Core.Infrastructure;
using Research.Core.Interface.Service;

namespace Research.Services.Tasks
{
    /// <summary>
    /// Bộ quản lý tác vụ, đọc danh sách các tác vụ trong database, sau đó gom nhóm các tác vụ này theo thời gian lặp lại. Mỗi nhóm
    /// như thế sẽ được gom lại để tạo các Task cho 1 đối tượng TaskThread quản lý => Như vậy có bao nhiêu loại khoảng thời gian sẽ
    /// có bây nhiêu TaskThread được tạo
    /// </summary>
    public partial class TaskManager
    {
        private static readonly TaskManager _taskManager = new TaskManager();

        /// <summary>
        /// Sẽ chỉ có duy nhất 1 đối tượng TaskManager được tạo ( singleton ), chịu trách nhiệm quản lý tác vụ, sinh các TaskThread để
        /// quản lý các Task
        /// </summary>
        public static TaskManager Instance
        {
            get { return _taskManager; }
        }
        
        private readonly IList<TaskThread> _taskThreads = new List<TaskThread>();

        /// <summary>
        /// danh sách các TaskThread đang có, mỗi cái sẽ có 1 bộ đếm thời gian và quản lý các Task riêng.
        /// Lúc gọi nhớ gán ra 1 biến nếu cần dùng nhiều lần
        /// </summary>
        public IList<TaskThread> TaskThreads
        {
            get { return new ReadOnlyCollection<TaskThread>(_taskThreads); }
        }

        /// <summary>
        /// Qui định thời gian chờ tối đa, những task có thời gian chờ vượt quá sẽ đc xử lý riêng
        /// </summary>
        private int _notRunTasksInterval = 60 * 30; // 30 phút

        private TaskManager()
        {
        }

        /// <summary>
        /// Khởi chạy tất cả các TaskThread đang được quản lý. Điều này đc thực hiện = cách gọi đến hàm khởi tạo bộ đếm thời gian
        /// </summary>
        public void Start()
        {
            foreach (var taskThread in _taskThreads)
                taskThread.InitTimer();
        }

        /// <summary>
        /// Dừng tất cả các TaskThread đang được quản lý. Điều này đc thực hiện = cách gọi đến hàm hủy các bộ đếm thời gian
        /// </summary>
        public void Stop()
        {
            foreach (var taskThread in _taskThreads)
                taskThread.Dispose();
        }

        /// <summary>
        /// Hàm khởi tạo task manager với những cấu hình lấy từ file cấu hình. Đọc về danh sách các task trong database
        /// và gom nhóm, tạo các nhóm taskThread cần thiết
        /// 
        /// Có thể gọi nhiều lần, nếu trong quá trình chạy của ứng dụng cấu hình task có thay đổi
        /// </summary>
        public void Initialize()
        {
            Stop(); //// khác ở đây : dừng các task đang có nếu có, nếu ta ko làm điều này sẽ khiến cho các taskThread vẫn sẽ chạy
            // Ta stop thì ít nhất cũng sẽ chỉ chỉ "để xổng" lần chạy cuối cùng, ko stop có thể khiến các taskThread cũ ko bị hủy và chạy song song
            _taskThreads.Clear();

            var taskService = EngineContext.Current.Resolve<IScheduleTaskService>();
            var scheduleTasks = taskService.GetAllTasks().OrderBy(p => p.Seconds)
                .ThenByDescending(p => p.RunWithAnotherTask).ToList();

            // gom nhóm các Itask theo thời gian chờ chạy lại
            TaskThread taskThread = null;
            ScheduleTask prevTask = null;
            foreach (var scheduleTask in scheduleTasks)
            {
                if (taskThread == null || scheduleTask.Seconds != taskThread.Seconds ||
                    !scheduleTask.RunWithAnotherTask || !prevTask.RunWithAnotherTask)
                {
                    taskThread = new TaskThread { Seconds = scheduleTask.Seconds };
                    _taskThreads.Add(taskThread);
                }
                taskThread.AddTask(new Task(scheduleTask));
                prevTask = scheduleTask;
            }

            //sometimes a task period could be set to several hours (or even days).
            //in this case a probability that it'll be run is quite small (an application could be restarted)
            //we should manually run the tasks which weren't run for a long time

            // tìm ra những task thuộc loại có thời gian chờ dài và chưa được chạy trong khoảng thời gian
            // >= _notRunTasksInterval. Với những task này, ngoại trừ việc xếp lịch chạy thông thường ( task sẽ được chạy lần đầu tiên
            // sau Second giây ( >= _notRunTasksInterval ) kể từ thời điểm appStart, chúng ta sẽ xếp thêm 1 lịch chạy riêng để ưu
            // tiên cho các tast này chạy DUY NHẤT 1 lần sau thời điểm App Start được 5 phút, điều này là để ĐỀN BÙ cho task vì nó 
            // đã ko chạy lâu quá rồi
            var notRunTasks = scheduleTasks.Where(t => t.Seconds >= _notRunTasksInterval && (
                !t.LastStartUtc.HasValue || t.LastStartUtc.Value.AddSeconds(_notRunTasksInterval) < DateTime.UtcNow)
                ).ToList();
            //create a thread for the tasks which weren't run for a long time
            if(notRunTasks.Count > 0)
            {
                taskThread = new TaskThread 
                { 
                    RunOnlyOnce = true,
                    Seconds = 5 * 60 // 5 phút
                };
                foreach (var task in notRunTasks)
                    taskThread.AddTask(new Task(task));
                _taskThreads.Add(taskThread);
            }
        }
    }
}
