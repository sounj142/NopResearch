using Research.Web.Framework.Mvc.Routes;
using System.Web.Routing;
using Research.Web.Framework.Localization;

namespace Research.Plugin.Shipping.CanadaPost
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapLocalizedRoute("MyTestUrl",
                "que-huong/hahaha",
                new { controller = "Canada", action = "Index" },
                new[] { "Research.Plugin.Shipping.CanadaPost.Controllers" }
            );
        }

        public int Priority
        {
            get { return -1; }
        }
    }
}
