using System;

namespace Research.Core.Interface.Data
{
    public enum TransactionState
    {
        Running,
        Committed,
        Rollbacked
    }

    /// <summary>
    /// Chỉ cần đặt trong using(), Transaction sẽ tự rollback ở hàm Dispose() nếu trước đó ko có hàm rollback rõ ràng. Vậy cho nên sẽ ko cần
    /// bắt try...catch để có thể Rollback
    /// </summary>
    public interface IWorkTransactionScope: IDisposable
    {
        TransactionState CurrentState { get; }

        void Commit();

        void Rollback();
    }
}
