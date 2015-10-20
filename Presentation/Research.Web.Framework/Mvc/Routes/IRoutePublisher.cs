using System.Web.Routing;

namespace Research.Web.Framework.Mvc.Routes
{
    /// <summary>
    /// sẽ có duy nhất 1 đối tượng cài đặt giao diện này, và chịu trách nhiệm tìm và đăng ký các route của hệ thống và của các plugin.
    /// Lớp đc tạo ra thông qua DI, và gọi đến RegisterRoutes để đăng ký route trong app start
    /// Lớp sẽ tìm trong app domain tất cả các lớp có cài đặt giao diện IRouteProvider, tạo ra thể hiện của chúng, sắp thứ tự và 
    /// lần lượt gọi đến hàm RegisterRoutes() của chúng
    /// </summary>
    public interface IRoutePublisher
    {
        /// <summary>
        /// Hàm chịu trách nhiệm đăng ký các route với RouteCollection, sẽ được gọi từ hàm đăng ký route trong lớp /App_Start/RouteConfig.cs
        /// </summary>
        void RegisterRoutes(RouteCollection routes);
    }
}
