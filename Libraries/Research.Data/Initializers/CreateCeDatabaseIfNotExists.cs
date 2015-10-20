using System;
using System.Data.Entity;
using System.Transactions;

namespace Research.Data.Initializers
{
    /// <summary>
    /// 1 Cài đặt của IDatabaseInitializer mà nó sẽ tạo và có tùy chọn sinh dữ liệu mẫu cho database nếu như database chưa tồn tại.
    /// Để sinh dữ liệu, tạo ra 1 lớp kế thừa và override Seed method
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class CreateCeDatabaseIfNotExists<TContext> : SqlCeInitializer<TContext> where TContext: DbContext
    {
        public override void InitializeDatabase(TContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            var replaceContext = ReplaceSqlCeConnection(context);

            bool databaseExists;
            using(new TransactionScope(TransactionScopeOption.Suppress))
            {
                // kiểm tra xem database đã tồn tại hay chưa
                databaseExists = replaceContext.Database.Exists();
            }
            if(databaseExists)
            {
                // kiểm tra xem cấu trúc của database đang có có phù hợp với mô hình trong dbcontext hay ko, nếu ko
                // hợp, báo lỗi ngay
                if(!context.Database.CompatibleWithModel(false))
                    throw new InvalidOperationException(string.Format("CreateCeDatabaseIfNotExists.InitializeDatabase: The model backing the '{0}' context has changed since the database was created. Either manually delete/update the database, or call Database.SetInitializer with an IDatabaseInitializer instance. For example, the DropCreateDatabaseIfModelChanges strategy will automatically delete and recreate the database, and optionally seed it with new data.", context.GetType().Name));
            }
            else
            {
                context.Database.Create(); // tạo databae từ context ( Code First )
                Seed(context); // sinh dữ liệu mẫu
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Override để add data vào database
        /// </summary>
        /// <param name="context"></param>
        protected virtual void Seed(TContext context)
        {

        }
    }
}
