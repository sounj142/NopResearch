using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Research.Core.Infrastructure;
using Research.Core.Data;
using Research.Web.Framework.Themes;
using Research.Services.Tasks;
using Research.Core.Interface.Service;
using Research.Core;
using Research.Web.Framework.Localization;

namespace Research.Web
{
    public class MvcApplication : HttpApplication
    {
        // Ghi chú: Từ giờ, ta sẽ qui định rằng nếu hệ thống hỗ trợ phân đoạng ngôn ngữ trong url thì phân đoạn đó sẽ có 2 ký tự,
        // và tất cả mọi phân đoạn có 2 ký tự đầu tiên của url ( sau app path ) đều sẽ đc coi là phân đoạn ngôn ngữ, bất kể nó là
        // gì đi nữa => cần đảm bảo trong các link seo, các controller - action, ko có bất kỳ 1 link nào dùng phân đoạn 2 ký tự
        // ở đầu


        /// Ghi nhớ: Có 1 đoạn cấu hình trong ~/Views/web.config liên quan đến việc ngăn chặn model validation sau khi đối tượng
        /// đã valid qua controller


        protected void Application_Start()
        {
            var enginer = EngineContext.Current;// gọi khởi tạo IEngine, tuy nhiên thao tác này hơi thừa vì EngineContext.Current cũng sẽ
            // thực hiện điều này nếu cần thiết
            

            // kiểm tra file Settings.txt có đầy đủ thông tin cấu hình cần thiết ( tên data provider và chuỗi kết nối ). Nếu có
            // đầy đù thông tin thì chứng tỏ thao tác khởi thạo lần đầu tiên của ứng dụng Nop đã đc thực hiện
            var dataSetting = enginer.Resolve<DataSettings>();
            bool databaseInstalled = dataSetting != null && dataSetting.IsValid();
            if (databaseInstalled)
            {
                // loại bỏ View enginer Razor mặc định để thay thế bằng 1 Enginer mới kế thừa từ VirtualPathProviderViewEngine và cung cấp 1 số khả năng
                // bổ sung như : Thay đổi đường dẫn mặc định tìm file view .cshtml, hỗ trợ thay đổi nhiều theme, hỗ trợ các method thuận tiện
                ViewEngines.Engines.Clear();
                ViewEngines.Engines.Add(new ThemeableRazorViewEngine());
                // chú ý: Để thêm 1 theme mới thì cần copy file web.config từ ~/Views/ vào gốc thư mục theme, đồng thời phải thêm file 
                // _ViewStart.cshtml để cấu hình layout mặc định, có thể thêm file layout.cshtml nếu muốn dùng layout
                // thứ tự tìm là thư mục ~/themes/ rồi mới đến thư mục ~/Views/, nên những view có ở ~/themes/ sẽ override view có ở 
                //  ~/Views/
            }

            // đăng ký routing
            AreaRegistration.RegisterAllAreas(); // hàm này sẽ khiến MVC tìm tất cả các lớp kế thừa AreaRegistration và tiến hành đăng ký area + routing ? 
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // 1 số bước ở đây


            
            if(databaseInstalled)
            {
                //start scheduled tasks
                // ghi chú: Các tác vụ được phân nhóm theo thời gian chờ, mỗi nhóm như thế sẽ được thực hiện tuần tự trong 1 Thread riêng
                // ( được kích hoạt chạy lặp lại sau thời gian chờ đó ) => Nếu muốn tác vụ được ưu tiên chạy riêng, nên đặt cho nó 1 thời gian
                // chờ duy nhất
                TaskManager.Instance.Initialize();
                TaskManager.Instance.Start();


                // ghi log cho biết ứng dụng được khởi chạy
                var logger = enginer.Resolve<ILogger>();
                if (logger != null)
                    logger.Information("NopResearch: Application Start...");
            }
        }

        protected void Application_End()
        {
            var logger = EngineContext.Current.Resolve<ILogger>();
            if (logger != null)
                logger.Information("NopResearch: Application Stop...Stop...Stop...");
        }

        protected void Application_BeginRequest()
        {
            var engine = EngineContext.Current;
            var webHelper = engine.Resolve<IWebHelper>();
            // nếu request hiện hành là 1 static request thì dừng phần xử lý BeginRequest tại đây, ko làm tiếp các thao tác kế tiếp nữa
            if (webHelper.IsStaticResource(this.Request)) return;

            // nếu request là keepAlive hoặc các thao tác cần thực hiện nhanh khác


            // là request thông thường ?
            // tiến hành rewrite lại AppRelativeCurrentExecutionFilePath 1 lần duy nhất ở ngay begin request để đảm bảo Routing hoạt động ok
            var urlStandardize = engine.Resolve<ILanguageUrlStandardize>();
            urlStandardize.RewriteAppRelativeCurrentExecutionFilePath();
        }
    }
}
