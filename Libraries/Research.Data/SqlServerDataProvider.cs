using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.Hosting;
using Research.Core.Data;
using Research.Data.Initializers;

namespace Research.Data
{
    /// <summary>
    /// Data provider dùng để thiết lập kết nối EF với sql server, và khởi tạo CSDL ban đầu cho database SQL 
    /// 
    /// Đối với CSDL là Sql Server, chúng ta sẽ sử dụng thủ tục khỏi tạo CreateTablesIfNotExist, trong đó kiểm tra nếu database
    /// chưa có bảng nào, hoặc ko có các bảng trong danh sách yêu cầu thì sẽ tạo mới database, tạo các index, và thực thi
    /// truy vấn sql tạo các store/function cần thiết để thao tác nhanh trên database
    /// </summary>
    public class SqlServerDataProvider : IDataProvider
    {
        #region Methods
        public virtual void InitConnectionFactory()
        {
            var connectionFactory = new SqlConnectionFactory();
            //TODO fix compilation warning (below)
            #pragma warning disable 0618
            Database.DefaultConnectionFactory = connectionFactory;

            File.WriteAllText(@"D:\bbbbbb.txt", "Duoc chay ne");
        }

        public virtual void SetDatabaseInitializer()
        {
            // 1 vài tên bảng để kiểm tra rằng chúng ta đăng sử dụng nop phiên bnar 2.x ( những bảng này sẽ chỉ có trên nop 2.x )
            var tablesToValidate = new[] { "Customer", "Discount", "Order", "Product", "ShoppingCartItem" };

            // lấy lên các lệnh custom cần thực thi ( store, index )
            var customCommands = new List<string>();
            customCommands.AddRange(ParseCommands(HostingEnvironment.MapPath("~/App_Data/Install/SqlServer.Indexes.sql"), false));
            customCommands.AddRange(ParseCommands(HostingEnvironment.MapPath("~/App_Data/Install/SqlServer.StoredProcedures.sql"), false));

            var initializer = new CreateTablesIfNotExist<NopObjectContext>(tablesToValidate, customCommands.ToArray());
            Database.SetInitializer(initializer);
        }

        public virtual void InitDatabase()
        {
            InitConnectionFactory();
            SetDatabaseInitializer();
        }

        public virtual bool StoredProceduredSupported
        {
            get { return true; }
        }

        public virtual DbParameter GetParameter()
        {
            return new SqlParameter();
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Chịu trách nhiệm đọc và trả về danh sách các lệnh sql từ 1 file filePath. Cấu trúc file này tổ chức các lệnh sql phân tách nhau
        /// bởi dòng chứa từ "GO"
        /// </summary>
        protected virtual IList<string> ParseCommands(string filePath, bool throwExceptionIfNonExists)
        {
            if(!File.Exists(filePath))
            {
                if(throwExceptionIfNonExists)
                    throw new ArgumentException(string.Format("Specified file doesn't exist - {0}", filePath));
                return new string[0];
            }

            var result = new List<string>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                string sqlQuery;
                while((sqlQuery = ReadNextStatementFromStream(reader)) != null)
                {
                    sqlQuery = sqlQuery.Trim();
                    if (!string.IsNullOrEmpty(sqlQuery)) result.Add(sqlQuery);
                }
            }

            return result;
        }
        /// <summary>
        /// Chịu trách nhiệm đọc ra lệnh sql kế tiếp từ reader ( các lệnh cách nhau bởi dòng có từ "GO" )
        /// </summary>
        protected virtual string ReadNextStatementFromStream(StreamReader reader)
        {
            var builder = new StringBuilder();
            while(true)
            {
                string line = reader.ReadLine();
                if (line == null) return builder.Length > 0 ? builder.ToString() : null;
                if (string.Equals(line.TrimEnd(), "GO", StringComparison.InvariantCultureIgnoreCase)) return builder.ToString();
                builder.AppendLine(line);
            }
        }

        #endregion
    }
}
