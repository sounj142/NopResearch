using System;
using Research.Core.Configuration;
using Research.Core.Infrastructure.DependencyManagement;

namespace Research.Core.Infrastructure
{
    /// <summary>
    /// Đóng vai trò trái tim của Nop, thực hiện đăng ký dependency injection, chạy tác vụ lúc app start ( Xem NopEngine )
    /// 
    /// 1 lớp cài đặt IEngine cần phải có 1 hàm tạo ko tham số để có thể được tạo động
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Đối tương ContainerManager, chịu trách nhiệm bao bọc lấy các chức năng phát sinh đối tượng sử dung Autofac, bởi đối
        /// tượng này biết phạm vị scope mặc định mà ứng dụng sẽ sử dụng.
        /// </summary>
        ContainerManager ContainerManager { get; }

        /// <summary>
        /// Từ thông tin cấu hình trong config, khởi tạo những thành phần và plugin của ứng dụng ( load assembly, 
        /// cấu hình kế nối database, ...v.v.v...)
        /// Nên chạy 1 lần lúc App Start ? Đây là method quan trọng nhất của inteface IEngine
        /// </summary>
        void Initialize(NopConfig config);

        /// <summary>
        /// Giải quyết sự phụ thuộc, là hàm thuận tiện mà về bản chất sẽ gọi đến this.ContainerManager.Resolve
        /// </summary>
        T Resolve<T>() where T : class;

        /// <summary>
        ///  Resolve dependency
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolve dependencies
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns></returns>
        T[] ResolveAll<T>() where T:class;
    }
}
