using System;
using System.Data.Entity;
using System.Transactions;

namespace Research.Data.Initializers
{
    /// <summary>
    /// 1 cài đặt của IDatabaseInitializer mà nó luôn luôn [tạo lại database và tùy chọn sinh dữ liệu mẫu] trong lần đầu tiên mà context
    /// được sử dụng trong appdomain 
    /// Để tạo dữ liệu mẫu, tạo 1 lớp kế thừa và override phương thức seed
    /// </summary>
    public class DropCreateCeDatabaseAlways<TContext> : SqlCeInitializer<TContext> where TContext: DbContext
    {

        public override void InitializeDatabase(TContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            var replacedContext = ReplaceSqlCeConnection(context);
            bool databaseExists;
            using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                databaseExists = replacedContext.Database.Exists();
            }
            if (databaseExists)
            {
                replacedContext.Database.Delete();
            }

            context.Database.Create();
            Seed(context);
            context.SaveChanges();
        }

        protected virtual void Seed(TContext context)
        {

        }
    }
}
