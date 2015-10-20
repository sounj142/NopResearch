using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;

namespace Research.Data.Initializers
{
    /// <summary>
    /// Chịu trách nhiệm kiểm tra sự tồn tại của các bảng đc chỉ định trong database, nếu chưa có thì tạo mới, đồng thời thực thi
    /// sql đính kèm nếu có ( lưu ý là database phải tồn tại nếu ko sẽ ném ngoại lệ )
    /// 
    /// Việc cài đặt IDatabaseInitializer cho phép lớp có thể được sử dụng bởi EF để tạo database nếu chưa có thông qua
    /// việc gọi phương thức InitializeDatabase()
    /// </summary>
    public class CreateTablesIfNotExist<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        private readonly string[] _tablesToValidate;
        private readonly string[] _customCommands;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablesToValidate">Danh sách các tên bảng cần phải kiểm tra, null nếu ko muốn kiểm tra tên bảng</param>
        /// <param name="customCommands">Các lệnh sql custom cần được chạy bổ sung sau thao tác tạo database ?</param>
        public CreateTablesIfNotExist(string[] tablesToValidate, string [] customCommands)
        {
            this._tablesToValidate = tablesToValidate;
            this._customCommands = customCommands;
        }

        /// <summary>
        /// Được triệu gọi để kiểm tra tạo các bảng, thực thi truy vấn sql bổ sung nếu có
        /// </summary>
        /// <param name="context"></param>
        public void InitializeDatabase(TContext context)
        {
            bool dbExists;
            using(new TransactionScope(TransactionScopeOption.Suppress))
            {
                dbExists = context.Database.Exists();
            }
            if (!dbExists) throw new ApplicationException("CreateTablesIfNotExist.InitializeDatabase: No database");

            bool createTables;
            if(_tablesToValidate != null && _tablesToValidate.Length>0)
            {
                // kiểm tra các table đc yêu cầu có tồn tại hay ko

                // lấy về danh sách các bảng trong database
                var existingTableNames = context.Database.SqlQuery<string>("SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE'").ToList();

                // nếu trong danh sách các bảng đang có của database ko có bất cứ bảng nào trong danh sách yêu cầu thì mới yêu cầu 
                // phát sinh các bảng
                createTables = !existingTableNames.Intersect(_tablesToValidate, StringComparer.InvariantCultureIgnoreCase).Any();
            }
            else
            {
                //check whether tables are already created
                int numberOfTables = 0;
                foreach (var t1 in context.Database.SqlQuery<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE' "))
                    numberOfTables = t1;
                // nếu ko có chỉ định kiểm tra tên bảng thì sẽ chỉ tạo mới các bảng nếu hiện trong database ko có bảng nào
                createTables = numberOfTables == 0;
            }

            if (createTables)
            {
                //create all tables
                var dbCreationScript = ((IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript();
                context.Database.ExecuteSqlCommand(dbCreationScript);

                //Seed(context);
                context.SaveChanges();

                if (_customCommands != null && _customCommands.Length > 0)
                {
                    foreach (var command in _customCommands)
                        context.Database.ExecuteSqlCommand(command);
                }
            }
        }
    }
}
