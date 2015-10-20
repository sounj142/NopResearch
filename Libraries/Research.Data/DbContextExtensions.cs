using System;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Research.Core;

namespace Research.Data
{
    public static class DbContextExtensions
    {
        private static T InnerGetCopy<T>(IDbContext context, T currentCopy, Func<DbEntityEntry<T>, DbPropertyValues> func)
            where T:BaseEntity
        {
            var dbContext = CastOrThrow(context);
            DbEntityEntry<T> entry = GetEntityOrReturnNull(currentCopy, dbContext);

            T output = null;
            if(entry != null)
            {
                DbPropertyValues dbPropertyValues = func(entry);
                if (dbPropertyValues != null) output = dbPropertyValues.ToObject() as T;
            }
            return output;
        }

        /// <summary>
        /// Chịu trách nhiệm tìm trong bảng dữ liệu T của dbcontext, xem có nguyên mẫu của phiên bản copy currentCopy hay ko, nếu có
        /// thì trả về DbEntityEntry[T], ngược lại trả về null. Phiên bản copy được so sánh nhờ override lại toán từ == trên BaseEntity
        /// </summary>
        private static DbEntityEntry<T> GetEntityOrReturnNull<T>(T currentCopy, DbContext dbContext) where T:BaseEntity
        {
            return dbContext.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity == currentCopy);
        }

        private static DbContext CastOrThrow(IDbContext context)
        {
            var db = context as DbContext;
            if (db != null) return db;
            throw new InvalidOperationException("Context does not support operation.");
        }



        /// <summary>
        /// Loads the original copy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="currentCopy">The current copy.</param>
        /// <returns></returns>
        public static T LoadOriginalCopy<T>(this IDbContext context, T currentCopy) where T:BaseEntity
        {
            return InnerGetCopy(context, currentCopy, e => e.OriginalValues);
        }

        /// <summary>
        /// Loads the database copy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="currentCopy">The current copy.</param>
        /// <returns></returns>
        public static T LoadDatabaseCopy<T>(this IDbContext context, T currentCopy) where T : BaseEntity
        {
            return InnerGetCopy(context, currentCopy, e => e.GetDatabaseValues());
        }

        /// <summary>
        /// Xóa đi 1 bảng với tên bảng cho trước
        /// </summary>
        public static void DropPluginTable(this DbContext context, string tableName)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException("tableName");

            if(context.Database.SqlQuery<int>("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}", tableName).Any())
            {
                var sql = "DROP TABLE [" + tableName + "]"; ;
                context.Database.ExecuteSqlCommand(sql);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Lấy về tên bảng tương ứng với kiểu entity T
        /// </summary>
        public static string GetTableName<T>(this IDbContext context) where T:BaseEntity
        {
            //var tableName = typeof(T).Name;
            //return tableName;

            //this code works only with Entity Framework.
            //If you want to support other database, then use the code above (commented)

            var adapter = ((IObjectContextAdapter)context).ObjectContext;
            var storageModel = (StoreItemCollection)adapter.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
            var containers = storageModel.GetItems<EntityContainer>();
            var entitySetBase = containers.SelectMany(c => c.BaseEntitySets.Where(bes => bes.Name == typeof(T).Name)).First();

            // Here are variables that will hold table and schema name
            string tableName = entitySetBase.MetadataProperties.First(p => p.Name == "Table").Value.ToString();
            //string schemaName = productEntitySetBase.MetadataProperties.First(p => p.Name == "Schema").Value.ToString();
            return tableName;
        }

        /// <summary>
        /// Lấy về chiều dài tối đa của 1 cột trong CSDL ( kiểu varchar/nvarchar ? ), null nếu ko có max length
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityTypeName">Tên kiểu Entity</param>
        /// <param name="columnName">Tên cột</param>
        /// <returns></returns>
        public static int? GetColumnMaxLength(this IDbContext context, string entityTypeName, string columnName)
        {
            //original: http://stackoverflow.com/questions/5081109/entity-framework-4-0-automatically-truncate-trim-string-before-insert
            int? result = null;

            Type entType = Type.GetType(entityTypeName);
            var adapter = ((IObjectContextAdapter)context).ObjectContext;
            var metadataWorkspace = adapter.MetadataWorkspace;
            var q = from meta in metadataWorkspace.GetItems(DataSpace.CSpace).Where(m => m.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                    from p in (meta as EntityType).Properties.Where(p => p.Name == columnName && p.TypeUsage.EdmType.Name == "String")
                    select p;

            var queryResult = q.Where(p =>
            {
                bool match = p.DeclaringType.Name == entityTypeName;
                if (!match && entType != null)
                {
                    //Is a fully qualified name....
                    match = entType.Name == p.DeclaringType.Name;
                }

                return match;

            }).Select(sel => sel.TypeUsage.Facets["MaxLength"].Value);

            if (queryResult.Any())
            {
                result = Convert.ToInt32(queryResult.First());
            }

            return result;
        }
    }
}
