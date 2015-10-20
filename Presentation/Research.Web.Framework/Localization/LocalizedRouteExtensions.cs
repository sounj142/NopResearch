using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Research.Web.Framework.Localization
{
    public static class LocalizedRouteExtensions
    {
        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url)
        {
            return MapLocalizedRoute(routes, name, url, null /* defaults */, (object)null /* constraints */);
        }
        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, object defaults)
        {
            return MapLocalizedRoute(routes, name, url, defaults, (object)null /* constraints */);
        }
        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, object defaults, object constraints)
        {
            return MapLocalizedRoute(routes, name, url, defaults, constraints, null /* namespaces */);
        }
        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, string[] namespaces)
        {
            return MapLocalizedRoute(routes, name, url, null /* defaults */, null /* constraints */, namespaces);
        }
        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
        {
            return MapLocalizedRoute(routes, name, url, defaults, null /* constraints */, namespaces);
        }

        /// <summary>
        /// Tạo ra và đăng ký 1 LocalizedRoute với routes, sử dụng danh sách các tham số đi kèm, trả về kết quả là 1 đối tượng Route
        /// được tạo ra
        /// </summary>
        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, 
            object defaults, object constraints, string[] namespaces)
        {
            if (url == null) throw new ArgumentNullException("url");
            var route = new LocalizedRoute(url, new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(defaults), // trước giờ chưa biết hàm này, toản phải reflection thủ công >"<
                Constraints = new RouteValueDictionary(constraints),
                DataTokens = new RouteValueDictionary()
            };
            if (namespaces != null && namespaces.Length > 0)
                route.DataTokens["Namespaces"] = namespaces; // cung cấp giới hạn về namespace cho routeHandler biết thông qua tập tokens
            routes.Add(name, route);
            return route;
        }

        /// <summary>
        /// Hàm chịu trách nhiệm xóa giá trị _seoFriendlyUrlsForLanguagesEnabled về null ở tất cả các đối tượng thuộc kiểu LocalizedRoute
        /// nằm trong RouteCollection, cho phép đọc lại giá trị setting này khi dữ liệu setting bị thay đổi
        /// </summary>
        public static void ClearSeoFriendlyUrlsCachedValueForRoutes(this RouteCollection routes)
        {
            foreach(var route in routes)
            {
                var localizedRoute = route as LocalizedRoute;
                if (localizedRoute != null) localizedRoute.ClearSeoFriendlyUrlsCachedValue();
            }
        }
    }
}
