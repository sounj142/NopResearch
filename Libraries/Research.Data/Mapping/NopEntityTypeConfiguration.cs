using System.Data.Entity.ModelConfiguration;

namespace Research.Data.Mapping
{
    /// <summary>
    /// Bằng cách kế thừa EntityTypeConfiguration, chúng ta có thể bổ sung những cấu hình cần thiết để tạo bảng, khóa chính, khóa ngoại,
    /// kiểu dữ liệu, ràng buộc toàn vẹn, ....v....v... để Entity Framework tạo Database 
    /// Quá trình đăng ký các NopEntityTypeConfiguration với EF sẽ được thực hiện dựa vào 1 kỹ thuật phù hợp
    /// 
    /// Các lớp kế thừa từ NopEntityTypeConfiguration bắt buộc phải có 1 hàm tạo public không tham số để có thể là 1 lớp cấu hình hợp lệ
    /// Thứ 2, chỉ có những lớp kế thừa trực tiếp từ NopEntityTypeConfiguration mới đc công nhận là lớp config
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NopEntityTypeConfiguration<T> : EntityTypeConfiguration<T> where T:class
    {
        protected NopEntityTypeConfiguration()
        {
            PostInitialize();
        }
        /// <summary>
        /// Có thể override lại method này để add 1 số code khởi tạo vào hàm tạo
        /// </summary>
        protected virtual void PostInitialize()
        {

        }
    }
}
