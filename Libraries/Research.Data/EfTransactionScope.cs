using Research.Core;
using Research.Core.Interface.Data;
using System;
using System.Data;
using System.Data.Entity;

namespace Research.Data
{
    public class EfTransactionScope : DisposableObject, IWorkTransactionScope
    {
        private DbContextTransaction transaction;

        public TransactionState CurrentState { get; private set; }

        public EfTransactionScope(DbContext context)
        {
            CurrentState = TransactionState.Running;
            transaction = context.Database.BeginTransaction();
        }

        public EfTransactionScope(DbContext context, IsolationLevel isolationlevel = IsolationLevel.Unspecified)
        {
            CurrentState = TransactionState.Running;
            transaction = context.Database.BeginTransaction(isolationlevel);
        }

        public void Commit()
        {
            if (CurrentState != TransactionState.Running) throw new Exception("Không gọi Commit quá 1 lần hoặc gọi sau Rollback");
            CurrentState = TransactionState.Committed;
            transaction.Commit();
        }

        public void Rollback()
        {
            if (CurrentState != TransactionState.Running) throw new Exception("Không gọi Rollback quá 1 lần hoặc gọi sau Commit");
            CurrentState = TransactionState.Rollbacked;
            transaction.Rollback();
        }

        protected override void DisposeCore()
        {
            if (CurrentState == TransactionState.Running)
            {
                Rollback(); // Rollback nếu ko có commit/rollback rõ ràng
            }
            transaction.Dispose();
        }
    }
}