using Research.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Định nghĩa interface chứa các hàm cơ bản cho service, các Service cụ thể, tùy theo nhu cầu sẽ định nghĩa thêm các method
    /// khác hoặc override lại các method đã có
    /// </summary>
    public interface IBaseService<T> where T : BaseEntity
    {
        void Delete(T entity);

        void Delete(Expression<Func<T, bool>> where);

        void Delete(IList<T> entities);

        T GetById(params object[] keyValues);

        T GetById(int id);

        IQueryable<T> GetAll();

        IQueryable<T> GetAllNoTracking();

        /// <summary>
        /// Insert đối tượng mới vào database
        /// </summary>
        void Insert(T entity);

        /// <summary>
        /// Insert 1 danh sách đối tượng mới vào database
        /// </summary>
        void Insert(IList<T> entities);

        /// <summary>
        /// Update dữ liệu cho đối tượng vào database
        /// Điều kiện gọi là entity được truyền vào phải là đối tượng được lấy ra trực tiếp từ EF
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Update dữ liệu cho các đối tượng vào database
        /// Điều kiện gọi là các entity được truyền vào phải là đối tượng được lấy ra trực tiếp từ EF
        /// </summary>
        void Update(IList<T> entities);

        IQueryable<T> IncludeProperties(IQueryable<T> anotherData, params Expression<Func<T, object>>[] includeProperties);

        IQueryable<T> IncludeProperties(IQueryable<T> anotherData, params string[] includePaths);
    }
}
