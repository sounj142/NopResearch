using System;
using System.Data.Entity;
using System.Transactions;

namespace Research.Data.Initializers
{
    /// <summary>
    /// 1 caì đặt của IDatabaseInitializer mà nó sẽ xóa và tạo lại database ( có thể sinh dữ liệu mẫu ), nếu model đã bị thay đổi 
    /// so với database hiện có. Làm điều này = cách viết 1 hash của model lưu trữ xuống database khi nó đc tạo ra và sau đó so sánh
    /// cái hash này với cái đc phát sinh từ model hiện hành
    /// </summary>
    public class DropCreateCeDatabaseIfModelChanges<TContext> : SqlCeInitializer<TContext> where TContext: DbContext
    {


        protected virtual void Seed(TContext context)
        {
        }

        public override void InitializeDatabase(TContext context)
        {
            if(context == null) throw new ArgumentNullException("context");
            var replacedContext = ReplaceSqlCeConnection(context);

            bool databaseExists;
            using(var transaction = new TransactionScope(TransactionScopeOption.Suppress))
            {
                databaseExists = replacedContext.Database.Exists();
            }
            if(databaseExists)
            {
                if (context.Database.CompatibleWithModel(true)) return;
                replacedContext.Database.Delete();
            }

            context.Database.Create();
            Seed(context);
            context.SaveChanges();
        }
    }
}
