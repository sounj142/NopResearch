using System.Linq;
using System.Collections.Generic;
using Research.Core.Domain.Tasks;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;

namespace Research.Services.Tasks
{
    public partial class ScheduleTaskService: BaseService<ScheduleTask>, IScheduleTaskService
    {
        public ScheduleTaskService(IRepository<ScheduleTask> repository): base(repository, null)
        { }

        public ScheduleTask GetTaskByType(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return null;
            return _repository.Table.Where(p => p.Type == type)
                .OrderByDescending(p => p.Id).FirstOrDefault();
        }

        public IQueryable<ScheduleTask> GetAllTasks(bool showHidden = false)
        {
            var datas = _repository.Table;
            if (!showHidden) datas = datas.Where(p => p.Enabled);
            return datas.OrderByDescending(p => p.Seconds);
        }
    }
}
