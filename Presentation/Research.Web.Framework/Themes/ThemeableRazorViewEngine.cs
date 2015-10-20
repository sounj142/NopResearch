using System.Collections.Generic;
using System.Web.Mvc;

namespace Research.Web.Framework.Themes
{
    /// <summary>
    /// Override lại ViewEngine để thay đổi các thư mục nơi sẽ tìm kiếm view và hỗ trợ cơ chế theme, tuy nhiên vẫn dùng
    /// RazorView như là IView để render view cho nên vẫn tận dụng được mọi sức mạnh của cú pháp razor
    /// </summary>
    public class ThemeableRazorViewEngine : ThemeableVirtualPathProviderViewEngine
    {

        /// <summary>
        /// Chú ý là cách định nghĩa chuỗi đường dẫn tĩnh ở đây đã khiến cho cấu hình theme của phần NopConfig trong 
        /// web.config không còn giá trị
        /// </summary>
        public ThemeableRazorViewEngine()
        {
            // đường dần tìm view khi có area
            AreaViewLocationFormats = new[] 
            {
                // 1: đường dẫn mới ( được ưu tiên tìm trước 2 )
                "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",

                // 2: đường dẫn theo tổ chức thư mục view nguyên thủy của Razor
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };
            AreaMasterLocationFormats = new[] // đường dẫn cho file layout
            {
                //themes
                "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",


                //default
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
            };
            // đường dẫn tim partial view khi có area
            AreaPartialViewLocationFormats = new[]
            {
                //themes
                "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",
                                                    
                //default
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };
            // đường dẫn tìm view thông thường
            ViewLocationFormats = new[]
            {
                //themes
                "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                "~/Themes/{2}/Views/Shared/{0}.cshtml",

                //default
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml",

                //Admin
                "~/Administration/Views/{1}/{0}.cshtml",
                "~/Administration/Views/Shared/{0}.cshtml",
            };
            MasterLocationFormats = new[]
            {
                //themes
                "~/Themes/{2}/Views/{1}/{0}.cshtml", 
                "~/Themes/{2}/Views/Shared/{0}.cshtml", 

                //default
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml"
            };
            PartialViewLocationFormats = new[]
            {
                //themes
                "~/Themes/{2}/Views/{1}/{0}.cshtml",
                "~/Themes/{2}/Views/Shared/{0}.cshtml",

                //default
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml", 

                //Admin
                "~/Administration/Views/{1}/{0}.cshtml",
                "~/Administration/Views/Shared/{0}.cshtml",
            };

            FileExtensions = new[] { "cshtml" }; // chỉ cho phép dạng file cshtml
        }

        /// <summary>
        /// Hàm tạo ra partial view từ 1 đường dẫn cụ thể, sử dụng Razor để render view
        /// </summary>
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return new RazorView(controllerContext, partialPath, null, false, this.FileExtensions);
        }

        /// <summary>
        /// Hàm tạo ra view từ 1 đường dẫn cụ thể và layout nếu có, sử dụng Razor để render view
        /// </summary>
        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return new RazorView(controllerContext, viewPath, masterPath, true, this.FileExtensions);
        }
    }
}
