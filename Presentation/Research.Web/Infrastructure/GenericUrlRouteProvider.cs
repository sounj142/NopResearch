using System.Web.Routing;
using Research.Web.Framework.Localization;
using Research.Web.Framework.Mvc.Routes;
using Research.Web.Framework.Seo;

namespace Research.Web.Infrastructure
{
    public partial class GenericUrlRouteProvider: IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            // đăng ký mẫu url khái quát dùng để phân giải Url đến với loại có 1 phân đoạn và đa ngôn ngữ. Route này được đặt sau tất cả
            // các route thông dụng, cho nên chỉ khi ko thể nào phân giải được ở các route trước đó, code mới chạy đến route này

            // việc phân giải url ở view sẽ ko đi vào route này, vì route đòi hỏi 1 phân đoạn tên rất đặc biệt: "generic_se_name",
            // sẽ ko có 1 yêu cầu nào ở view dùng tên phân đoạn "kỳ lạ" như thế này
            routes.MapGenericPathRoute("GenericUrl", 
                                        "{generic_se_name}",
                                        new { controller = "Common", action = "GenericUrl" }, 
                                        new[] { "Research.Web.Controllers" });

            // các route này chỉ dùng để tạo link trong các view chứ ko hề có ý nghĩa trong phân giải url đến. Tất cả các url đến
            // loại 1 phân đoạn nếu có thể đều sẽ đi vào GenericPathRoute ở phía trên
            routes.MapLocalizedRoute("Product",
                                     "{SeName}",
                                     new { controller = "Product", action = "ProductDetails" },
                                     new[] { "Research.Web.Controllers" });

            routes.MapLocalizedRoute("Category",
                            "{SeName}",
                            new { controller = "Catalog", action = "Category" },
                            new[] { "Research.Web.Controllers" });

            routes.MapLocalizedRoute("Manufacturer",
                            "{SeName}",
                            new { controller = "Catalog", action = "Manufacturer" },
                            new[] { "Research.Web.Controllers" });

            routes.MapLocalizedRoute("Vendor",
                            "{SeName}",
                            new { controller = "Catalog", action = "Vendor" },
                            new[] { "Research.Web.Controllers" });

            routes.MapLocalizedRoute("NewsItem",
                            "{SeName}",
                            new { controller = "News", action = "NewsItem" },
                            new[] { "Research.Web.Controllers" });

            routes.MapLocalizedRoute("BlogPost",
                            "{SeName}",
                            new { controller = "Blog", action = "BlogPost" },
                            new[] { "Research.Web.Controllers" });

            routes.MapLocalizedRoute("Topic",
                            "{SeName}",
                            new { controller = "Topic", action = "TopicDetails" },
                            new[] { "Research.Web.Controllers" });
        }

        public int Priority
        {
            get { return -1000000; }
        }
    }
}