using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Research.Core.Interface.Data
{
    /// <summary>
    /// Định nghĩa giao diện của repository sẽ được cài đặt trong tầng DTO, nhìn chung mỗi 1 bảng sẽ tương ứng với 1 lớp
    /// Repository cài đặt giao diện này
    /// 
    /// Ghi chú: Chỉ những lớp đòi hỏi cài đặt phức tạp, vượt quá khả năng của cài đặt generic EFRepository[T] như lớp Product
    /// thì ta mới cài đặt giao diện riêng cho nó, ngược lại cứ dùng IRepository[T] và thể hiện EFRepository[T] là đủ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial interface IRepository<T> where T : BaseEntity
    {
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Lấy về 1 đối tượng theo Id
        /// </summary>
        T GetById(params object[] keyValues);

        /// <summary>
        /// Insert đối tượng mới vào database
        /// </summary>
        T Insert(T entity);

        /// <summary>
        /// Insert 1 danh sách đối tượng mới vào database
        /// </summary>
        void Insert(IEnumerable<T> entities);

        /// <summary>
        /// Update dữ liệu cho đối tượng vào database
        /// Điều kiện gọi là entity được truyền vào phải là đối tượng được lấy ra trực tiếp từ EF
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Update dữ liệu cho các đối tượng vào database
        /// Điều kiện gọi là các entity được truyền vào phải là đối tượng được lấy ra trực tiếp từ EF
        /// </summary>
        void Update(IEnumerable<T> entities);

        /// <summary>
        /// Xóa đối tượng khỏi database
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Xóa 1 danh sách đối tượng khỏi database
        /// </summary>
        void Delete(IEnumerable<T> entities);

        /// <summary>
        /// Lấy về bảng ở dạng queryable, tương ứng với cú pháp db.Products
        /// </summary>
        IQueryable<T> Table { get; }

        /// <summary>
        /// Lấy về bảng, nhưng dữ liệu sẽ không được theo vết bởi EF. Sử dụng nó nếu muốn lấy data và sau đó thoải mái sử dụng
        /// mà ko sợ bị lưu lại ở thao tác SaveChanges()
        /// </summary>
        IQueryable<T> TableNoTracking { get; }

        IQueryable<T> IncludeProperties(IQueryable<T> anotherData, params Expression<Func<T, object>>[] includeProperties);

        IQueryable<T> IncludeProperties(IQueryable<T> anotherData, params string[] includePaths);


        // Hàm này nguyên bản được cài đặt bởi Research.Data.Extensions, tuy nhiên vì nhận thấy điều này khiên cho 
        // Research.Service phụ thuộc vào Research.Data nên ta quyết định sẽ đưa nó vào như là 1 thành phần của IRepository

        /// <summary>
        /// Nếu EF là cho phép proxy, runtime sẽ trả về kết quả là 1 đối tượng proxy thay vì đối tượng thực thể. VD như
        /// 1 truy vấn lấy về Product thông qua EF sẽ ko thực sự là 1 product mà là 1 abc_xyz_Product nào đó kế thừa từ Product,
        /// và override lại các virtual property bằng cách thêm vào những code đặc biệt dùng cho những mục đích như theo vết
        /// và lazy loading. VD như 1 property số nhiều tương ứng với khóa ngoại trỏ từ 1 bảng khác, sẽ đc override lại
        /// để trở thành lazy loading, và sẽ chỉ thực sự đc thực hiện ( truy vấn CSDL ) khi có thao tác gọi trực tiếp đến nó
        /// 
        /// Hàm này trả về tên thực sự của đối tượng nguyên bản, thay vì tên của đối tượng proxy
        /// </summary>
        /// <param name="entity">Đối tượng thực thể do EF trả về, thường là 1 lớp nặc danh kế thừa từ lớp thực thể</param>
        /// <returns>Trả về kiểu đúng của đối tương entity nguyên bản vd như kiểu Product</returns>
        Type GetUnproxiedEntityType(BaseEntity entity);
    }
}
