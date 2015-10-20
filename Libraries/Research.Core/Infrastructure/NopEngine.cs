using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Research.Core.Configuration;
using Research.Core.Infrastructure.DependencyManagement;

namespace Research.Core.Infrastructure
{
    // ĐÂY LÀ TRÁI TIM CỦA NOP
    // việc tạo NopEngine và chạy NopEngine.Initialize() với vai trò IEngine được thực hiện bới EngineContext.CreateEngineInstance


    /// <summary>
    /// Engine của cả ứng dụng. Chịu trách nhiệm đăng ký sự phụ thuộc, tìm và chạy các tác vụ, là singleton trong app
    /// được đảm bảo bởi Autofac
    /// </summary>
    public class NopEngine : IEngine 
    {
        #region Fields

        /// <summary>
        /// Đối tượng chịu trách nhiệm tạo đối tượng bằng kỹ thuật DI - Autofact. Do thể hiện của NopEngine được lưu vào Autofact 
        /// duy nhất nên _containerManager cũng là duy nhất
        /// </summary>
        private ContainerManager _containerManager;

        #endregion

        #region Methods
        public ContainerManager ContainerManager
        {
            get { return _containerManager; }
        }

        /// <summary>
        /// Thực hiện những bước cần thiết để đăng ký dependency injection với autofac và khởi chạy các task cần thiết lúc app start
        /// 
        /// 
        /// </summary>
        /// <param name="config">Đối tượng lưu cấu hình hệ thống của nop</param>
        public void Initialize(NopConfig config)
        {
            RegisterDependencies(config); // đăng ký sự phụ thuộc với Autofact, sau đó đăng ký với MVC
            if (!config.IgnoreStartupTasks) RunStartupTasks(); // chạy những tác vụ startup nếu có yêu cầu
        }

        /// <summary>
        /// Giải quyết sự phụ thuộc, là hàm thuận tiện mà về bản chất sẽ gọi đến this.ContainerManager.Resolve
        /// Có thể trực tiếp sử dụng IEngine.ContainerManager để có thể gọi đến những hàm chuyên dụng hơn
        /// </summary>
        public T Resolve<T>() where T : class
        {
            return ContainerManager.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return ContainerManager.Resolve(type);
        }

        public T[] ResolveAll<T>() where T:class
        {
            return ContainerManager.ResolveAll<T>();
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Hàm chạy những tác vụ phù hợp ở thời điểm start ứng dụng
        ///
        /// Tìm kiếm tất cả những lớp cài đặt giao diện IStartupTask trên toàn bộ appdomain 
        /// và gọi hàm Execute() của chúng theo thức tự cho bởi Order
        /// </summary>
        protected virtual void RunStartupTasks()
        {
            // yêu cầu autofac trả về 1 đối tượng ITypeFinder dùng để tìm các kiểu phù hợp tiêu chí tìm kiếm trong app domain
            var typeFinder = _containerManager.Resolve<ITypeFinder>();
            // tìm tất cả các lớp có cài đặt giao diện IStartupTask trong toàn bộ app domain
            var startUpTaskTypes = typeFinder.FindClassesOfType<IStartupTask>();

            // Tạo ra các đối tượng IStartupTask tương ứng, sắp xếp theo thứ tự cho bởi Order và khởi chạy chúng
            var startUpTasks = startUpTaskTypes.Select(t => (IStartupTask)Activator.CreateInstance(t)).OrderBy(p => p.Order).ToList();
            foreach (var startUpTask in startUpTasks)
                startUpTask.Execute();
        }

        /// <summary>
        /// Đăng ký những sự phụ thuộc ( DI của autofact )
        /// 
        /// Là trái tim nơi thực hiện đăng ký dependency injection của toàn bộ ứng dụng
        /// Chỉ nên gọi 1 lần duy nhất lúc App Start
        /// </summary>
        /// <param name="config">Thông tin cấu hình ứng dụng</param>
        protected virtual void RegisterDependencies(NopConfig config)
        {
            var builder = new ContainerBuilder();
            IContainer container = builder.Build(); // lấy về đối tượng IContainer cần thiết để gọi các hàm Resolve ?

            //we create new instance of ContainerBuilder
            //because Build() or Update() method can only be called once on a ContainerBuilder.


            var typeFinder = new WebAppTypeFinder(config); // tạo mới chỉ 1 lần duy nhất ở đây
            // đăng ký sự phụ thuộc quan trọng của 3 kiểu NopConfig, IEngine, ITypeFinder
            builder = new ContainerBuilder();
            builder.RegisterInstance(config).As<NopConfig>().SingleInstance(); // đối tượng config sẽ là duy nhất singleton của type NopConfig trên toàn bộ app
            builder.RegisterInstance(this).As<IEngine>().SingleInstance(); // tự đăng ký this là singleton IEngine với autofact
            builder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance(); // WebAppTypeFinder là singleton của ITypeFinder
            builder.Update(container);

            // đăng ký sự phụ thuộc đc cung cấp bởi những assembly khác, sửa dụng cơ chế động hoàn toàn, bằng cách tìm tất cả các 
            // lớp cài đặt IDependencyRegistrar
            builder = new ContainerBuilder();
            // thao tác gọi đến FindClassesOfType() lần đầu tiên này sẽ kích hoạt việc load các dll trong thư mục bin vào app domain
            var drTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();

            var drInstances = drTypes.Select(p => (IDependencyRegistrar)Activator.CreateInstance(p)).OrderBy(q => q.Order).ToList();
            foreach (var drInstance in drInstances)
                drInstance.Register(builder, typeFinder);
            builder.Update(container); // update thông tin đăng ký vào container

            // Tạo đối tượng giải quyết sự phụ thuộc
            this._containerManager = new ContainerManager(container);

            // đăng ký container là bộ giải quyết sự phụ thuộc Dependency injection với MVC
            // đây chính là thao tác sẽ thay thế bộ DI mặc định của MVC ( hóa ra là giấu ở đây, ko phải trong global.asax )
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        #endregion
    }
}
