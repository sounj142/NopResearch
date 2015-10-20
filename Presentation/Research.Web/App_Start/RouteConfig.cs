using Research.Core.Infrastructure;
using Research.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Research.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // tạo thể hiện của IRoutePublisher và dùng nó để đăng ký tất cả các route mà hệ thống ( giao diện font-end ) dùng
            var routePublisher = EngineContext.Current.Resolve<IRoutePublisher>();
            routePublisher.RegisterRoutes(routes);

            // đóng vai trò route mặc định, được đặt ở cuối cùng
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Research.Web.Controllers" }
            );
        }
    }
}
