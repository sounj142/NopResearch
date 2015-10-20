using System.Web.Routing;

namespace Research.Web.Framework.Mvc.Routes
{
    /// <summary>
    /// Bất cứ lớp nào muốn đăng ký route với RouteCollection đều cần cài đặt giao diện này. Giao diện có 1 hàm, sẽ là nơi ta đăng ký route
    /// và có 1 số thứ tự để bộ điều phối IRoutePublisher có thể sắp xếp thứ tự đăng ký vào RouteCollection
    /// Bằng cách này các plugin cũng có thể tự đăng ký những route của riêng mình, chỉ cần chú ý thứ tự để ko xung đột với những route khác
    /// </summary>
    public interface IRouteProvider
    {
        /// <summary>
        /// Hàm đăng ký route
        /// </summary>
        /// <param name="routes"></param>
        void RegisterRoutes(RouteCollection routes);

        /// <summary>
        /// Thứ tự, dùng để sắp xếp thứ tự đăng ký vào RouteCollection nếu có nhiều IRouteProvider
        /// Điều đặc biệt là thứ tự này được sắp giảm dần, nghĩa là càng lớn càng đc đưa lên đầu
        /// </summary>
        int Priority { get; }
    }
}
