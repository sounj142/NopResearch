using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Research.Services.Tasks
{
    /// <summary>
    /// Quản lý các tác vụ cần chạy .
    /// Sẽ chỉ có 1 Thread duy nhất để chạy tất cả các Task được lưu trong nó.
    /// Dùng 1 bộ đếm Timer để lặp lại, sau mỗi khoảng thời gian Interval sẽ lặp lại việc foreach qua tất cả các Task và gọi thực thi
    /// hàm Excute()
    /// </summary>
    public partial class TaskThread : IDisposable
    {
        #region field, property and ctor

        /// <summary>
        /// Bộ đếm thời gian đóng vai trò là đồng hồ của lớp ?
        /// </summary>
        private Timer _timer;
        private bool _disposed;
        /// <summary>
        /// Như đã nói, Task sẽ đại diện cho các ScheduleTask trong database => Mỗi key cho bởi ScheduleTask.Type sẽ được hiện thực hóa
        /// bới 1 đối tượng Task. Task đến phiên mình sẽ tự lo việc gọi đến ITask.Excute ( ITask là đối tượng của kiểu ScheduleTask.Type )
        /// </summary>
        private readonly IDictionary<string, Task> _tasks;

        internal TaskThread()
        {
            _tasks = new Dictionary<string, Task>();
        }

        /// <summary>
        /// Khoảng thời gian theo giây để chạy tác vụ. Tức là sau khi hoàn tất 1 vòng lặp gọi thực thi các tác vụ, ta sẽ đi vào nghỉ ngơi
        /// và thiết lập khoảng thời gian phải chờ để thức dậy làm công việc tiếp theo ?
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Ngày tháng mà thread được khởi chạy
        /// </summary>
        public DateTime StartedUtc { get; private set; }

        /// <summary>
        /// Chỉ ra thread có đang chạy hay ko
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Lấy về danh sách các task được quản lý bởi đối tượng
        /// </summary>
        public IList<Task> Tasks
        {
            get { return new ReadOnlyCollection<Task>(this._tasks.Values.ToList()); }
        }

        /// <summary>
        /// Thời gian ngủ theo mili giây để cho lần chạy task kế tiếp
        /// </summary>
        public int Interval
        {
            get { return Seconds * 1000; }
        }

        /// <summary>
        /// Chỉ ra Thread có nên chạy chỉ 1 lần hay ko ( 1 lần là vào lúc App Start ). Mặc định false
        /// </summary>
        public bool RunOnlyOnce { get; set; }

        #endregion

        #region methods

        public void Dispose()
        {
            Dispose(true);
            //if(!_disposed) GC.SuppressFinalize(this);
        }

        ~TaskThread()
        {
            Dispose(false);
        }

        private void Dispose(bool isDisposing)
        {
            if (!_disposed && isDisposing)
            {
                Timer timer = null;
                if (_timer != null)
                {
                    lock (this)
                    {
                        if (_timer != null)
                        {
                            timer = _timer;
                            _timer = null;
                        }
                    }
                }

                // hủy bộ đếm thời gian nếu vẫn đang tiếp tục chờ, giống như khi chúng ta hủy setInterval trong javascript
                if (timer != null) timer.Dispose();
            }
            _disposed = true;
        }

        /// <summary>
        /// Nơi lặp qua các ITask và chạy lần lượt chúng
        /// </summary>
        private void Run()
        {
            if (Seconds <= 0) return;
            this.StartedUtc = DateTime.UtcNow;
            this.IsRunning = true;
            foreach (var task in _tasks.Values) task.Execute(); // gọi thưc thi với tham số mặc định
            this.IsRunning = false;
        }

        private void TimerHandler(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite); // yêu cầu timer dừng việc lặp lại. Điều này để đề phòng các tác vụ
            // thực thi tốn quá hiều thời gian khiến cho lần chạy thứ 2, thứ 3 đến ngay trong khi mà lần chạy thưc nhất chua xong
            // Thứ nữa, điều này giúp dừng bộ đếm, đảm bảo cho tại thời điểm mà các tác vụ thức hiện xong, có thể chạy lại bộ đếm
            // và đếm đúng khoảng thời gian gọi lại TimerHandler()

            this.Run(); // thực thi tất cả các ITask (tuần tự)
            if (this.RunOnlyOnce) this.Dispose(true); // dừng bộ đếm thời gian nếu chỉ thực thi các ITask 1 lần lúc appstart
            else _timer.Change(Interval, Interval); // thiết đặt lại bộ đếm thời gian
        }

        /// <summary>
        /// Khởi tạo bộ đếm thời gian
        /// </summary>
        public void InitTimer()
        {
            // đặt giờ để sau cứ sau thời gian Interval sẽ thức dậy hàm TimerHandler, lặp lại mãi cho đến khi Dispose được gọi
            // , gần giống setInterval trong javascript

            // Use a TimerCallback delegate to specify the method that is called by a Timer. This method does not execute in the thread 
            // that created the timer; it executes in a separate thread pool thread that is provided by the system. The TimerCallback
            // delegate invokes the method once after the start time elapses, and continues to invoke it once per timer interval until 
            // the Dispose method is called, or until the Timer.Change method is called with the interval value Infinite.
            // https://msdn.microsoft.com/en-us/library/system.threading.timercallback%28v=vs.110%29.aspx
            if (_timer == null)
                _timer = new Timer(new TimerCallback(TimerHandler), null, Interval, Interval);
        }

        public void AddTask(Task task)
        {
            // dùng tên do người dùng đặt để đặt tên cho Task chứ ko dùng Type ?
            // Cũng có lý, cho phép chạy cùng lúc nhiều task có cùng 1 kiểu ?
            if(!this._tasks.ContainsKey(task.Name)) this._tasks.Add(task.Name, task);
        }

        #endregion
    }
}
