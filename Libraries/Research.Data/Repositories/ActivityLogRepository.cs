using Research.Core.Data;
using Research.Core.Domain.Logging;
using Research.Core.Interface.Data;
using System.Linq;

namespace Research.Data.Repositories
{
    public partial class ActivityLogRepository : EfRepository<ActivityLog>, IActivityLogRepository
    {
        private readonly IDataProvider _dataProvider;

        public ActivityLogRepository(IUnitOfWork unitOfWork, IDataProvider dataProvider)
            :base (unitOfWork)
        {
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// Thao tác này sẽ save change luôn vì nó giả định rằng có thể được thực hiện bằng cách triệu gọi stored proc
        /// </summary>
        public virtual void ClearAllActivities(bool useStoredProceduresIfSupported)
        {
            if (useStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //although it's not a stored procedure we use it to ensure that a database supports them
                //we cannot wait until EF team has it implemented - http://data.uservoice.com/forums/72025-entity-framework-feature-suggestions/suggestions/1015357-batch-cud-support

                //do all databases support "Truncate command"?
                string logTableName = _context.GetTableName<ActivityLog>();
                //_context.ExecuteSqlCommand(string.Format("TRUNCATE TABLE [{0}]", logTableName));
                _context.ExecuteSqlCommand(string.Format("DELETE FROM [{0}]", logTableName));
            }
            else
            {
                var allLog = Entities.ToList();
                Delete(allLog);
                _unitOfWork.SaveChanges(); // gọi để đồng bộ với trường hợp chạy câu lệnh ở trên
            }
        }
    }
}
