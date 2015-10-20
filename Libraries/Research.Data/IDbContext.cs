using System.Collections.Generic;
using System.Data.Entity;
using Research.Core;
using System.Data.Entity.Infrastructure;

namespace Research.Data
{
    /// <summary>
    /// Giao diện cho phép chuyển 1 lớp dbContext cụ thể, chẳng hạn MobishopDbContext thành 1 giao diện tổng quát IDbContext,
    /// cho phép ko cần phải quan tâm đến vấn đề kết nối database, có thể coi data như 1 IDbContext và truy xuất thông qua các
    /// IQueryable
    /// 
    /// Sử dụng IDbContext sẽ cho phép dùng DI để tạo ra lớp Context cần thiết, loại bỏ sự phụ thuộc của hệ thống vào 1 lớp Context
    /// cụ thể, cho phép bên thứ 3 có thể custom lại 1 dbcontext khác, miễn là họ tuân theo giao diện IDbContext
    /// </summary>
    public interface IDbContext
    {
        /// <summary>
        /// Lấy về DbSet, tương ứng với 1 table, hoặc câu truy vấn select * from TableName
        /// </summary>
        IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity;

        /////// <summary>
        /////// Hàm savechange của dbcontext
        /////// </summary>
        /////// <returns></returns>
        ////int SaveChanges();

        /// <summary>
        /// Thực hiện store proc và trả về kết quả là 1 list entity.
        /// Điều gì sẽ xảy ra nếu store phải trả về kết quả khác TEntity, 1 giá trị int output ? 1 kết quả join của nhiều bảng ?
        /// </summary>
        IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters)
            where TEntity : BaseEntity, new();

        /// <summary>
        /// Tạo ra 1 câu truy vấn sql từ danh sách tham số, và trả về kết quả thuộc kiểu TElement. Kiểu này có thể là bất cứ kiểu gì
        /// mà có property khớp với tên của những cột trả về bởi truy vấn, hoặc có thể đơn giản là kiểu nguyên thủy. Kiểu ko bắt buộc
        /// phải là 1 entity. 
        /// 
        /// Kết quả của truy vấn sẽ ko được theo vết bởi EF cho dù kiểu trả về là entity
        /// </summary>
        /// <typeparam name="TElement">Kiểu của đối tượng đc trả về bởi truy vấn</typeparam>
        /// <param name="sql">câu truy vấn</param>
        /// <param name="parameters">danh sách tham số </param>
        /// <returns></returns>
        IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters);

        /// <summary>
        /// Thực hiện truy vấn delete/update dữ liệu
        /// </summary>
        /// <param name="sql">câu lệnh</param>
        /// <param name="doNotEnsureTransaction">false: việc tạo transaction ko đc đảm bảo</param>
        /// <param name="timeout">Timeout value, theo giây. Null tức là sẽ sử dụng giá trị mặc định của provide bên dưới</param>
        /// <param name="parameters">tham số của sql</param>
        /// <returns></returns>
        int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters);

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}
