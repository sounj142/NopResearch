using Research.Core.Domain.Logging;

namespace Research.Core.Interface.Data
{
    public partial interface ILoggerRepository: IRepository<Log>
    {
        /// <summary>
        /// Hàm cho phép xóa log bằng store proc nếu có hỗ trợ hoặc dùng Entity Framework
        /// Ghi chú: Do có thể có store proc nên để đồng bộ, hàm sẽ luôn gọi save change.
        /// Nếu muốn kiểm soát lại thì trên service nên dùng transaction 
        /// </summary>
        void ClearLog(bool useStoredProceduresIfSupported);
    }
}
