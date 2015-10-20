using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using Research.Core.Data;
using Research.Data.Initializers;

namespace Research.Data
{
    /// <summary>
    /// data provider dành cho CSDL Sql CE. Ta sẽ dùng DI để phát sinh CSDL này ( Resovel[IDataProvider]() ), và sử dụng nó để kết nối CSDL
    /// trong trường hợp dataprovide được chọn là Sql CE
    /// 
    /// Bộ IDataProvider nên được tạo và khởi chạy duy nhất 1 lần lúc app start
    /// </summary>
    public class SqlCeDataProvider : IDataProvider
    {
        /// <summary>
        /// Thiết lập lại Connection Factory cho EF
        /// </summary>
        public virtual void InitConnectionFactory()
        {
            var connectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
            //TODO fix compilation warning (below)
            #pragma warning disable 0618
            Database.DefaultConnectionFactory = connectionFactory;
        }

        /// <summary>
        /// Thiết lập cơ chế khởi tạo cho database, ở đây ta chọn CreateDatabaseIfNotExists, tức là hệ thống sẽ kiểm tra nếu chưa có 
        /// database thì sẽ tạo mới
        /// </summary>
        public virtual void SetDatabaseInitializer()
        {
            var initializer = new CreateCeDatabaseIfNotExists<NopObjectContext>();
            Database.SetInitializer(initializer);
        }

        /// <summary>
        /// Khởi tạo database theo 2 bước: thiết lập connectionFactory mới và thiết lập bộ khởi tạo ban đầu co database ( code first )
        /// </summary>
        public virtual void InitDatabase()
        {
            InitConnectionFactory();
            SetDatabaseInitializer();
        }

        public virtual bool StoredProceduredSupported
        {
            get { return false; }
        }

        public virtual DbParameter GetParameter()
        {
            return new SqlParameter();
        }
    }
}
