using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Research.Data
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Cho phép hỗ trợ nhanh method Include() mà ko cần include EF (using System.Data.Entity), cho phép thao tác select trên EF có thể
        /// lấy kèm theo các property khác mà ko để lazy loading, giúp tăng hiệu năng trong trường hợp ta biết chắc là cần
        /// dữ liệu tại các khóa ngoại nào
        /// </summary>
        public static IQueryable<T> IncludeProperties<T>(this IQueryable<T> queryable,
            params Expression<Func<T, object>>[] includeProperties)
        {
            if (queryable == null) throw new ArgumentNullException("queryable");
            if (includeProperties != null)
            {
                foreach (var func in includeProperties)
                    queryable = queryable.Include(func);
            }
            
            return queryable;
        }

        public static IQueryable<T> IncludeProperties<T>(this IQueryable<T> queryable,
            params string[] includePaths)
        {
            if (queryable == null) throw new ArgumentNullException("queryable");
            if (includePaths != null)
            {
                foreach (var path in includePaths)
                    queryable = queryable.Include(path);
            }
            
            return queryable;
        }
    }
}
