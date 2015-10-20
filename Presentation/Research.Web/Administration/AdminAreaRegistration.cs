using System.Web.Mvc;

namespace Research.Admin
{
    /// <summary>
    /// Rất đơn giản, chúng ta cài đặt 1 lớp kế thừa AreaRegistration để đăng ký với MVC 1 Area mà chúng ta muốn
    /// MVC sẽ tự search type trong app domain để tìm tất cả các kiểu kế thừa từ AreaRegistration, tạo thể hiện và triệu gọi đăng ký
    /// area cho chúng ta
    /// </summary>
    public class AdminAreaRegistration: AreaRegistration
    {
        public override string AreaName
        {
            // Area tên là Admin nhưng lại chứa trong thư mục ~/Administration. Việc cấu hình để mvc biết cách tìm vị trí các view/layout
            // cho các trang admin được cài đặt trong custom ViewEngine ( xem Research.Web.Framework.Themes.ThemeableVirtualPathProviderViewEngine )
            get { return "Admin"; }
        }

        /// <summary>
        /// Hàm đăng ký các route cho area này. Nếu có nhiều area cùng tồn tại thì MVC sẽ gọi chúng chạy lần lượt ?
        /// </summary>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            // area admin sẽ chỉ dùng 1 route duy nhất với phần đầu là Admin/ . Route này sẽ được đặt ở vị trí đầu tiên trong RouteCollection
            context.MapRoute("Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", area = "Admin", id = "" },
                new[] { "Research.Admin.Controllers" });
        }
    }
}