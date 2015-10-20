using System;
using Autofac;
using Research.Core.Domain.Tasks;
using Research.Core.Infrastructure;
using Research.Core.Interface.Service;
using Research.Services.Logging;

namespace Research.Services.Tasks
{
    // có bao nhiêu ScheduleTask sẽ có bấy nhiêu Task ? Task đại diện cho ScheduleTask
    // Task có 1 hàm Execute, trong đó, nó sẽ chịu trách nhiệm tạo ra đối tượng ITask tương ứng và gọi lại hàm Execute của đối tượng
    // này 1 cách đồng bộ, ngoài ra còn cập nhật lại các mốc thời gian chạy của ITask và lưu xuống ScheduleTask tương ứng

    /// <summary>
    /// Đại diện cho 1 tác vụ được chạy bởi hệ thống ( tác vụ cài đặt bởi ITask ) ?
    /// </summary>
    public partial class Task
    {
        /// <summary>
        /// Giá trị chỉ ra Task có đang chạy hay ko
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Ngày giờ của lần khởi chạy cuối cùng, tính theo giờ Utc
        /// </summary>
        public DateTime? LastStartUtc { get; private set; }

        /// <summary>
        /// Ngày giờ của lần kết thúc chạy cuối cùng, tính theo giờ Utc
        /// </summary>
        public DateTime? LastEndUtc { get; private set; }

        /// <summary>
        /// Ngày giờ của lần kết thúc chạy cuối cùng mà thành công, tính theo giờ Utc
        /// </summary>
        public DateTime? LastSuccessUtc { get; private set; }

        /// <summary>
        /// Tên kiểu đầy đủ của Task
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Có dừng việc chạy task khi lỗi hay ko
        /// </summary>
        public bool StopOnError { get; private set; }

        /// <summary>
        /// Tên của task ( tên người dùng đặt trong cột name database )
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Chỉ ra task có đc bật hay ko
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// private để ngăn chặn bên ngoài tự ý tạo task với ctor này. Thiết lập enable = true cho ctor
        /// </summary>
        private Task()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Khởi tạo task với 1 đối tượng ScheduleTask đến từ database
        /// </summary>
        public Task(ScheduleTask task)
        {
            this.Type = task.Type;
            this.Enabled = task.Enabled;
            this.StopOnError = task.StopOnError;
            this.Name = task.Name;
        }

        /// <summary>
        /// Từ thông tin cấu hình nội tại đang có, tạo ra 1 ITask tương ứng với kiểu Type, nếu Enable = false thì sẽ 
        /// ko tạo task mà trả về null. Quá trình tạo ITask sẽ được thực hiện dựa vào Autofac và hàm TryResolve/ResolveUnregistered
        /// </summary>
        private ITask CreateTask(ILifetimeScope scope)
        {
            ITask result = null;
            if(this.Enabled)
            {
                var taskType = System.Type.GetType(this.Type);
                if(taskType != null)
                {
                    object instance;
                    var container = EngineContext.Current.ContainerManager; // lấy về ContainerManager để triệu gọi các hàm DI nâng cao
                    // thử yêu cầu Autofac tạo đối tượng taskType theo cách thông thường, nếu ko được thì thử dùng hàm 
                    // ResolveUnregistered để cố tạo lần nữa
                    if (!container.TryResolve(taskType, out instance, null, scope))
                        instance = container.ResolveUnregistered(taskType, scope);
                    result = instance as ITask;
                }
            }

            return result;
        }

        /// <summary>
        /// Thực thi tác vụ với thông tin nội tại đang có
        /// </summary>
        /// <param name="throwException">ngoại lệ có đc ném ra khi có lỗi hay ko</param>
        /// <param name="dispose">thể hiên của ITask có cần được dispose sau khi chạy task hay ko</param>
        public void Execute(bool throwException = false, bool dispose = true)
        {
            this.IsRunning = true; // đánh dấu task đang chạy. Như thế này thì 1 lớp Task sẽ được tạo ra để thực thi 1 đối tượng
            // ITask từ table ScheduleTask ?

            // các background task có 1 vấn đề với Autofac, bởi vì scope đc phát sinh mỗi khi nó đc yêu cầu
            // đó là tại sao chúng ta lấy 1 scope đơn ở đây, bằng cách đó ta cũng sẽ hủy bỏ tài nguyên một khi task hoàn tất
            var container = EngineContext.Current.ContainerManager;
            var scope = container.Scope();
            // tạo đối tương IScheduleTaskService, là 1 service cho phép truy cập vào database
            var scheduleTaskService = container.Resolve<IScheduleTaskService>(null, scope);
            // đọc lại ScheduleTask từ database ? 
            var scheduleTask = scheduleTaskService.GetTaskByType(this.Type);

            try
            {
                // mỗi lần thực thi sẽ tạo mới 1 đối tượng ITask thông qua sự hỗ trợ của Autofac
                var task = this.CreateTask(scope); // tạo task với kiểu cho bởi this.Type, và giải quyết sự phụ thuộc nếu có thông qua autofac
                if(task != null)
                {
                    this.LastStartUtc = DateTime.UtcNow;
                    if(scheduleTask != null) // cập nhật lại thông tin về thời gian chạy của task vào database
                    {
                        scheduleTask.LastStartUtc = this.LastStartUtc;
                        scheduleTaskService.Update(scheduleTask);
                    }

                    task.Execute(); // thực thi ITask bằng cách gọi hàm qui ước. Quá trình thực thi này là đồng bộ, phải chăng 
                    // class Task này sẽ đại diện cho thread được thực thi bất đồng bộ ?
                    this.LastEndUtc = this.LastSuccessUtc = DateTime.UtcNow;
                }
            }catch(Exception err)
            {
                if (this.StopOnError) this.Enabled = false; // tắt nếu có cấu hình tắt khi có lỗi
                this.LastEndUtc = DateTime.UtcNow;

                // ghi log
                var logger = EngineContext.Current.ContainerManager.Resolve<ILogger>(null, scope);
                logger.Error(string.Format("Error while running the '{0}' schedule task. Type={1}",
                    this.Name, this.Type), err);

                if (throwException) throw err; // ném ngoại lệ nếu có yêu cầu
            }
            if (scheduleTask != null)
            {
                scheduleTask.LastEndUtc = this.LastEndUtc;
                scheduleTask.LastSuccessUtc = this.LastSuccessUtc;
                scheduleTaskService.Update(scheduleTask);
            }
            // dispose tất cả các tài nguyên, chưa hiểu lắm
            if (dispose) scope.Dispose();

            this.IsRunning = false;
        }
    }
}
