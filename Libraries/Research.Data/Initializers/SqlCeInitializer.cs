using System;
using System.Data.Entity;
using System.Data.SqlServerCe;
using System.IO;

namespace Research.Data.Initializers
{
    /// <summary>
    /// Lớp chịu trách nhiệm khởi tạo database có kiểu cho bởi T ?
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class SqlCeInitializer<TContext>: IDatabaseInitializer<TContext> where TContext:DbContext
    {
        public abstract void InitializeDatabase(TContext context);

        /// <summary>
        /// Nếu DBContext dùng 1 SqlCeConnection và có DataSource, trả về 1 DbContext mới với cùng 1 chuỗi kết nối SqlCE, nhưng với
        /// |DataDirectory| được thay thế bằng đường dẫn vật lý thực sự
        /// </summary>
        protected static DbContext ReplaceSqlCeConnection(DbContext context)
        {
            if(context.Database.Connection is SqlCeConnection)
            {
                var builder = new SqlCeConnectionStringBuilder(context.Database.Connection.ConnectionString);
                if(!string.IsNullOrWhiteSpace(builder.DataSource))
                {
                    builder.DataSource = ReplaceDataDirectory(builder.DataSource);
                    return new DbContext(builder.ConnectionString);
                }
            }
            return context;
        }

        private static string ReplaceDataDirectory(string inputString)
        {
            string str = inputString.Trim();
            if (string.IsNullOrEmpty(str) || !str.StartsWith("|DataDirectory|", 
                StringComparison.InvariantCultureIgnoreCase))
            {
                return str;
            }
            var data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(data))
            {
                data = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
            }
            if (string.IsNullOrEmpty(data))
            {
                data = string.Empty;
            }
            int length = "|DataDirectory|".Length;
            if ((str.Length > "|DataDirectory|".Length) && ('\\' == str["|DataDirectory|".Length]))
            {
                length++;
            }
            return Path.Combine(data, str.Substring(length));
        }
    }
}
