using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Integration.Mvc;

namespace Research.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// Lớp quản lý, bao bọc lại Autofac, cung cấp các method có dạng Resolve(), 
    /// cho phép tạo ra đối tượng thông qua kỹ thuật dependency injection sử dụng Autofac
    /// 
    /// Được tạo ra và đặt trong NopEngine._containerManager. Do NopEngine là singleton nên ContainerManager cũng là tồn tại singleton
    /// </summary>
    public class ContainerManager
    {
        private readonly IContainer _container;

        public IContainer Container
        {
            get { return _container; }
        }

        public ContainerManager(IContainer container)
        {
            this._container = container;
        }
        /// <summary>
        /// Cho phép tạo ra đối tượng sử dụng autofac
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key để phân biệt đối tượng nếu có, dùng trong trường hợp nhiều kiểu cùng đăng ký 1 interface</param>
        /// <param name="scope">Phạm vi vòng đời muốn lấy, nếu truyền vào null thì mặc định sẽ ưu tiên lấy phạm vi vòng đời request</param>
        /// <returns></returns>
        public T Resolve<T>(string key = null, ILifetimeScope scope = null) where T : class
        {
            if (scope == null) scope = Scope();
            if (string.IsNullOrEmpty(key)) return scope.Resolve<T>();
            return scope.ResolveKeyed<T>(key);
        }

        public object Resolve(Type type, ILifetimeScope scope = null, string key = null) //// khác ở đây
        {
            if (scope == null) scope = Scope();
            if (string.IsNullOrEmpty(key)) return scope.Resolve(type);
            return scope.ResolveKeyed(key, type);
        }
        /// <summary>
        /// Chịu trách nhiệm tạo 1 mảng các đối tượng thuộc kiểu T dùng DI
        /// </summary>
        public T[] ResolveAll<T>(string key = null, ILifetimeScope scope = null) where T: class //// khác ở đây
        {
            if (scope == null) scope = Scope();
            if (string.IsNullOrEmpty(key)) return scope.Resolve<IEnumerable<T>>().ToArray();
            return scope.ResolveKeyed<IEnumerable<T>>(key).ToArray();
        }

        public T ResolveUnregistered<T>(ILifetimeScope scope = null) where T: class
        {
            return ResolveUnregistered(typeof(T), scope) as T;
        }
        /// <summary>
        /// Hàm cố gắng giải quyết sự phụ thuộc cho 1 kiểu chưa được đăng ký với autofac. Tức là giả lập 1 phần công việc của Autofac
        /// </summary>
        public object ResolveUnregistered(Type type, ILifetimeScope scope = null)
        {
            if (scope == null) scope = Scope();
            foreach (var constructor in type.GetConstructors())
            {
                var parameterInstances = new List<object>();
                bool breaked = false;
                foreach (var parameter in constructor.GetParameters())
                {
                    var service = Resolve(parameter.ParameterType, scope);
                    if (service == null)
                    {
                        breaked = true;
                        break;
                    }
                    parameterInstances.Add(service);
                }
                if (!breaked) return Activator.CreateInstance(type, parameterInstances.ToArray());
            }
            throw new ResearchException("ContainerManager.ResolveUnregistered: No contructor was found that had all the dependencies satisfied.");
        }

        public bool TryResolve(Type serviceType, out object instance, string key = null, ILifetimeScope scope = null) //// khác ở đây
        {
            if (scope == null) scope = Scope();
            if (string.IsNullOrEmpty(key)) return scope.TryResolve(serviceType, out instance);
            return scope.TryResolveKeyed(key, serviceType, out instance);
        }

        public bool TryResolve<T>(out T instance, string key = null, ILifetimeScope scope = null) where T:class
        {
            if (scope == null) scope = Scope();
            if (string.IsNullOrEmpty(key)) return scope.TryResolve<T>(out instance);
            object outData;
            bool result = scope.TryResolveKeyed(key, typeof(T), out outData);
            instance = (T)outData;
            return result;
        }

        public bool IsRegistered(Type serviceType, ILifetimeScope scope = null, string key = null)
        {
            if (scope == null) scope = Scope();
            if (string.IsNullOrEmpty(key)) return scope.IsRegistered(serviceType);
            return scope.IsRegisteredWithKey(key, serviceType);
        }

        public bool IsRegistered<T>(ILifetimeScope scope = null, string key = null) where T:class
        {
            if (scope == null) scope = Scope();
            if (string.IsNullOrEmpty(key)) return scope.IsRegistered<T>();
            return scope.IsRegisteredWithKey<T>(key);
        }
        /// <summary>
        /// Lấy về 1 đối tượng thông qua DI, nhưng ko bắt buộc phải có, trả về null nếu ko giải quyết đc sự phụ thuộc ( kiểu
        /// ko được đăng ký trong autofac )
        /// </summary>
        public object ResolveOptional(Type serviceType, ILifetimeScope scope = null)
        {
            if (scope == null) scope = Scope();
            return scope.ResolveOptional(serviceType);
        }

        public T ResolveOptional<T>(ILifetimeScope scope = null, string key = null) where T:class
        {
            if (scope == null) scope = Scope();
            if (string.IsNullOrEmpty(key)) return scope.ResolveOptional<T>();
            return scope.ResolveOptionalKeyed<T>(key);
        }

        public ILifetimeScope Scope()
        {
            try
            {
                if (HttpContext.Current != null)
                    return AutofacDependencyResolver.Current.RequestLifetimeScope;
                //when such lifetime scope is returned, you should be sure that it'll be disposed once used (e.g. in schedule tasks)
                return Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            }catch(Exception)
            {
                //we can get an exception here if RequestLifetimeScope is already disposed
                //for example, requested in or after "Application_EndRequest" handler
                //but note that usually it should never happen

                //when such lifetime scope is returned, you should be sure that it'll be disposed once used (e.g. in schedule tasks)
                return Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            }
        }
    }
}
