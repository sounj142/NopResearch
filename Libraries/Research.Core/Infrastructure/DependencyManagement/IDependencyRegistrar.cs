using Autofac;

namespace Research.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// Những lớp nào muốn đăng ký sự phụ thuộc vào autofac ( ánh xạ interface-class ) thì chỉ cần cài đặt giao diện này, có 1 hàm tạo 
    /// public ko tham số, ngoài ra ko  phải làm gì cả
    /// 
    /// Tại thời điểm AppStart, IEngine của hệ thống sẽ tự động quét qua tất cả các assembly và tìm tất cả các lớp cài đặt IDependencyRegistrar
    /// Sau đó, sẽ tiến hành đăng ký ánh xạ dependency vào autofact bằng cách gọi hàm Register, theo thứ tự cho bởi Order
    /// </summary>
    public interface IDependencyRegistrar
    {
        /// <summary>
        /// Hàm dùng để đăng ký ánh xạ interface-class với autofact
        /// </summary>
        /// <param name="builder">builder của autofact ( đăng ký ánh xạ vào đây )</param>
        /// <param name="typeFinder">đối tượng cho phép tìm kiếm kiểu trong toàn bộ AppDomain nêu có nhu cầu dùng</param>
        void Register(ContainerBuilder builder, ITypeFinder typeFinder);

        /// <summary>
        /// Thứ tự gọi hàm Register() nếu tìm thấy nhiều lớp cài đặt IDependencyRegistrar
        /// </summary>
        int Order { get; }
    }
}
