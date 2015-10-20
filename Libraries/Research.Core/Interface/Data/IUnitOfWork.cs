using System;
using System.Data;

namespace Research.Core.Interface.Data
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges();

        object GetIDbContext();

        IWorkTransactionScope BeginTransaction();

        IWorkTransactionScope BeginTransaction(IsolationLevel isolationlevel);
    }
}
