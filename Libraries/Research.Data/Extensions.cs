using System;
using System.Data.Entity.Core.Objects;
using Research.Core;

namespace Research.Data
{
    public static class Extensions
    {
        // ghi chú: Cài đặt này bị hủy bỏ và đưa vào IRepository vì nó gây ra sự phụ thuộc trực tiếp của Research.Service vào Research.Data

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
        ////public static Type GetUnproxiedEntityType(this BaseEntity entity)
        ////{
        ////    return ObjectContext.GetObjectType(entity.GetType());
        ////}
    }
}
