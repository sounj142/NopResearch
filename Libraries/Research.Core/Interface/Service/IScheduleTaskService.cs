using System.Linq;
using Research.Core.Domain.Tasks;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Giao diện định nghĩa cho service thực hiện các thao tác thêm, xóa, sửa, tìm kiếm trên bảng task
    /// Nói chung đây là phần service nằm ở bên trên phần IRepository, không hơn ko kém
    /// </summary>
    public partial interface IScheduleTaskService
    {
        void Delete(ScheduleTask entity);

        ScheduleTask GetById(int id);

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        /// <returns>Task</returns>
        ScheduleTask GetTaskByType(string type);

        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Tasks</returns>
        IQueryable<ScheduleTask> GetAllTasks(bool showHidden = false);

        void Insert(ScheduleTask entity);

        void Update(ScheduleTask entity);
    }
}
