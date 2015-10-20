using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Research.Core.Infrastructure;
using Research.Core.Plugins;

namespace Research.Web.Framework.Mvc.Routes
{
    /// <summary>
    /// Lớp chịu trách nhiệm quét và tìm kiếm tất cả các lớp có cài đặt giao diện IRouteProvider và tiền hành đăng ký các route vào
    /// RouteCollection
    /// </summary>
    public class RoutePublisher : IRoutePublisher
    {
        protected readonly ITypeFinder typeFinder;

        public RoutePublisher(ITypeFinder typeFinder)
        {
            this.typeFinder = typeFinder;
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            var routeProviderTypeList = typeFinder.FindClassesOfType<IRouteProvider>();
            var routeProviders = new List<IRouteProvider>();
            foreach(var routeProviderType in routeProviderTypeList)
            {
                var pluginDescriptor = PluginManager.FindPlugin(routeProviderType);
                // bỏ qua ko đăng ký routing cho các IRouteProvider trực thuộc plugin ko được Installed
                if (pluginDescriptor != null && !pluginDescriptor.Installed) continue;

                var routeProvider = Activator.CreateInstance(routeProviderType) as IRouteProvider;
                if (routeProvider != null) routeProviders.Add(routeProvider);
            }
            routeProviders = routeProviders.OrderByDescending(p => p.Priority).ToList();
            routeProviders.ForEach(p => p.RegisterRoutes(routes));
        }
    }
}
