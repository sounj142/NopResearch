using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Research.Core;
using Research.Core.Data;
using Research.Data.Mapping;

using Research.Core.Interface.Data;

namespace Research.Data
{
    /// <summary>
    /// Đối tương DbContext của EF. Có thể cài đặt 1 lớp khác, dựa trên LINQ, NHineble, .v...v.. miễn là cũng cấp giao diện IDbContext
    /// 
    /// Cặt đăt để DBContext cũng chính là UnitOfWork. Dù sao thì UnitOfWork cũng chứa trong nó duy nhất 1 DBContext, cài đặt UnitOfWork
    /// cho DBContetx sẽ giúp UnitOfWork tận dụng được mọi khả năng của DbContext, nhất là transaction ( điều này khó
    /// nếu như unit of work phải cài đặt riêng và chỉ biết đến IDbContext )
    /// </summary>
    public class NopObjectContext : DbContext, IDbContext, IUnitOfWork
    {
        #region Hàm tạo

        public NopObjectContext(string nameOrConnectionString): base(nameOrConnectionString)
        { 
        }

        private static string DeminateConnectionString()
        {
            var dataProvider = new SqlServerDataProvider();
            dataProvider.InitConnectionFactory();

            var dataSettingsManager = new DataSettingsManager();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDirectory.Substring(0,
                baseDirectory.Length - @"Libraries\Research.Data\bin\Debug\".Length),
                @"Presentation\Research.Web\App_Data\Settings.txt");
            var dataProviderSettings = dataSettingsManager.LoadSettings(path);
            return dataProviderSettings.DataConnectionString;
        }

        public NopObjectContext()
            : base(DeminateConnectionString())
        {
        }

        #endregion

        #region Hỗ trợ

        /// <summary>
        /// Hàm được gọi trong quá trình khởi tạo DbContext. Nó sẽ dùng cơ chế động để lấy về danh sách tất cả các lớp kế thừa từ
        /// NopEntityTypeConfiguration, và đăng ký những lớp đó với modelBuilder. Bằng cách đó, những cấu hình database cần thiết 
        /// của các lớp domain sẽ được cấu hình đầy đủ, phục vụ cho cơ chế Code First
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // tìm tất cả những kiểu thuộc assembly hiện hành, có namespace, có lớp cha trực tiếp, và lớp cha này phải là NopEntityTypeConfiguration
            // nên cache/lưu static cái này lại vì sẽ hiếm khi thay đổi ?
            var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => !string.IsNullOrEmpty(type.Namespace) && type.BaseType != null
                    && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(NopEntityTypeConfiguration<>));

            foreach(var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Gắn 1 đối tượng vào trong context hoặc trả về 1 đối tượng đã đc gắn sẵn nếu như nó đã có trong context trước đó
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual TEntity AttachEntityToContext<TEntity>(TEntity entity) where TEntity: BaseEntity, new()
        {
            // hack 1 chút ở đây cho đến khi EF thục sự hỗ trợ stored proc
            var dbSet = Set<TEntity>();
            var alreadyAttached = dbSet.Local.FirstOrDefault(p => p.Id == entity.Id);
            if(alreadyAttached == null)
            {
                dbSet.Attach(entity);
                return entity;
            }
            return alreadyAttached;
        }

        #endregion

        #region IDataContext Methods

        /// <summary>
        /// Trả về script phát sinh database. Tức là hàm sẽ đi từ tất cả những cấu hình mà DBContext đang có để phát sinh ra
        /// 1 script có thể dùng để phát sinh ra database tương ứng
        /// </summary>
        public string CreateDatabaseScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }

        /// <summary>
        /// Lấy dbset. Có lẽ trong DBContext thì quan trọng và đc dùng trong Nop là cái này
        /// </summary>
        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }
        /// <summary>
        /// Thực thi Store Proc và trả về 1 danh sách các Entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="commandText">Lệnh ( tên store proc? )</param>
        /// <param name="parameters">Danh sách tham số ( phải có kiểu DbParameter ) </param>
        /// <returns></returns>
        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) 
            where TEntity : BaseEntity, new()
        {
            // add các parameters vào commandText
            if(parameters != null && parameters.Length>0)
            {
                var builder = new StringBuilder(commandText, commandText.Length + 256);
                for(int i=0; i<parameters.Length; i++)
                {
                    var parameter = parameters[i] as DbParameter;
                    if (parameter == null) throw new Exception("NopObjectContext.ExecuteStoredProcedureList: Not support parameter value null");
                    builder.Append(i == 0 ? " @" : ", @").Append(parameter.ParameterName);
                    if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
                        builder.Append(" output");
                }
                commandText = builder.ToString();
            }

            var result = this.Database.SqlQuery<TEntity>(commandText, parameters).ToList();
            //performance hack applied as described here - http://www.nopcommerce.com/boards/t/25483/fix-very-important-speed-improvement.aspx
            bool autoDetectChangesEnabled = this.Configuration.AutoDetectChangesEnabled; // lưu lại giá trị hiện hành
            try
            {
                // tắt chế độ tự động phát hiện thay đổi ( tracking ) để tăng hiệu năng, và vì lý do Store Proc trả về ko đủ kết quả ?
                this.Configuration.AutoDetectChangesEnabled = false;
                for (int i = 0; i < result.Count; i++) result[i] = AttachEntityToContext(result[i]);
            }finally
            {
                // khôi phục cài đặt gốc
                this.Configuration.AutoDetectChangesEnabled = autoDetectChangesEnabled;
            }
            return result;
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            return this.Database.SqlQuery<TElement>(sql, parameters);
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            int? previousTimeout = null;
            if (timeout.HasValue)
            {
                previousTimeout = ((IObjectContextAdapter)this).ObjectContext.CommandTimeout;
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = timeout;
            }
            try
            {
                var transactionalBehavior = doNotEnsureTransaction ?
                TransactionalBehavior.DoNotEnsureTransaction : TransactionalBehavior.EnsureTransaction;
                return this.Database.ExecuteSqlCommand(transactionalBehavior, sql, parameters);
            }
            finally
            {
                if (timeout.HasValue)
                {
                    ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = previousTimeout;
                }
            }
        }

        #endregion

        #region IUnitOfWork Methods

        public object GetIDbContext()
        {
            return this;
        }

        public IWorkTransactionScope BeginTransaction()
        {
            return new EfTransactionScope(this);
        }

        public IWorkTransactionScope BeginTransaction(IsolationLevel isolationlevel)
        {
            return new EfTransactionScope(this, isolationlevel);
        }

        #endregion
    }
}
