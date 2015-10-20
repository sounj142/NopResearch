using System;
using System.Web;
using System.Web.Routing;
using Research.Core.Data;
using Research.Core.Domain.Localization;
using Research.Core.Infrastructure;
using Research.Core;

namespace Research.Web.Framework.Localization
{
    // LocalizedRoute đã được sửa lại để làm việc với giả định rằng AppRelativeCurrentExecutionFilePath đã được rewrite lại để ko
    // còn phân đoạn ngôn ngữ trong đó => Nếu viết unit test cần nhớ điểm này. Nó khác với cài đặt của Nop khi mà mỗi Route đều mất
    // công lọc phân đoạn ngôn ngữ và đều đi RewritePath 1 cách ngu ngốc !




    /// <summary>
    /// Kế thừa lớp Route để tạo ra 1 Route cho phép có thêm phân đoạn ngôn ngữ, dạng /us/regiter, ngoài ra ko có gì khác
    /// Nó có 1 field cục bộ, _seoFriendlyUrlsForLanguagesEnabled, cho biết hệ thống có dùng phân đoạn ngôn ngữ cho url ko
    /// + Nếu false thì nó dùng lại mọi ứng xử của lớp base Route, chả có gì đặc biệt
    /// + Nếu true thì: Khi phân giải url tới, nó ghi đè lại AppRelativeCurrentExecutionFilePath để bỏ phân đoạn ngôn ngữ đi ( nhưng RawUrl
    /// vẫn ko đổi ), sau đó gọi lớp base Route để giải quyết mẫu AppRelativeCurrentExecutionFilePath ko còn phân đoạn ngôn ngữ theo cách bình thường
    /// Đối với Url đi, nó sẽ lấy về kết quả của lớp Route, sau đó thêm vào phân đoạn ngôn ngữ ở đằng trước ( lấy nhanh từ request.RawUrl )
    /// </summary>
    public class LocalizedRoute: Route
    {
        #region Fields, Properties and Ctors

        /// <summary>
        /// Sử dụng link đa ngôn ngữ dạng có hay ko có phân đoạn ngôn ngữ dạng /us/ ??
        /// </summary>
        private bool? _seoFriendlyUrlsForLanguagesEnabled;

        /// <summary>
        /// Cho biết thao tác GetRouteData có kèm theo kiểm tra url language segment ko
        /// </summary>
        protected virtual bool CheckSeoUrl
        {
            get { return true; }
        }

        protected bool SeoFriendlyUrlsForLanguagesEnabled
        {
            get
            {
                // vì các đối tượng routing được chứa trog RouteCollection và dùng chung cho toàn bộ ứng dụng, nên cần đặt khóa ở phần thiết lập giá trị
                // thứ nữa, do Route được dùng chung, nên thao tác set giá trị cho _seoFriendlyUrlsForLanguagesEnabled thường sẽ chỉ được thực hiện 1 lần,
                // sau đó sẽ ko được set nữa cho đến khi gọi đến hàm ClearSeoFriendlyUrlsCachedValue()
                // => Có nên thiết lập trực tiếp giá trị này trong hàm tạo, ko dùng lazy loading nữa, và khi Clear thì cũng đọc giá trị mới
                // vào để thay thế ngay ?

                // liệu có tình huống 1 request đang xử lý nửa chừng với mẫu link ko có phân đoạn ngôn ngữ, nhưng nửa sau lại có phân đoạn
                // ngôn ngữ. Lúc đó sẽ thế nào? Code có còn chạy đúng ko ?
                if(!_seoFriendlyUrlsForLanguagesEnabled.HasValue)
                {
                    // bản thân thao tác lấy giá trị cho LocalizationSettings cũng tương tác cache rất phức tạp nên việc đặt khóa 
                    // lock ở đây có thể tiềm ẩn rủi ro => cần xem xét kỹ
                    _seoFriendlyUrlsForLanguagesEnabled = EngineContext.Current.Resolve<LocalizationSettings>().SeoFriendlyUrlsForLanguagesEnabled;
                }
                return _seoFriendlyUrlsForLanguagesEnabled.Value;
            }
        }

        public LocalizedRoute(string url, IRouteHandler routeHandler)
            :base(url, routeHandler)
        {
        }

        public LocalizedRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults,routeHandler)
        {
        }

        public LocalizedRoute(string url, RouteValueDictionary defaults, 
            RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler)
        {
        }

        public LocalizedRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints,
            RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler)
        {
        }

        #endregion

        #region Methods
        // ****************************************** LƯU Ý
        // ** Đã test : Nop ko hề quan tâm đến phân đoạn ngôn ngữ có đúng hay ko, nó chỉ đơn giản xác định nếu có phân đoạn 2 ký tự ở đầu, đồng
        // thời cấu hình setting sử dụng phân đoạn ngôn ngữ được bật thế là nó ung dung bỏ ngay 2 ký tự và mặc nhiên coi đó là phân đoạn 
        // ngôn ngữ, chả cần so sánh coi đúng hay ko
        // Cho nên, nếu ta đang ở link http://localhost:15536/en/hp-pavilion-artist-edition-dv2890nr-141-inch-laptop,
        // và sửa lại thành http://localhost:15536/qq/hp-pavilion-artist-edition-dv2890nr-141-inch-laptop
        // thì đừng ngạc nhiên khi toàn bộ link trong trang đều bị chuyển thành /qq/.... mà vẫn hoạt động tốt

        // phân đoạn ngôn ngữ chỉ cư xử đúng duy nhất trong trường hợp nó là đúng với ngôn ngữ của đoạn link đi theo sau nó
        // khi đó, phân đoạn ngôn ngữ sẽ gây ra chuyển đổi ngôn ngữ sang ngôn ngữ khác nếu cần thiết
        // vd nếu bạn đang ở link tiếng anh, 2 url sau sẽ đều bị redirect về trang tiếng Anh:
        // localhost:15536/qq/may-tinh-hp-phien-ban-dep
        // localhost:15536/en/may-tinh-hp-phien-ban-dep
        // nhưng link sau sẽ ok và đổi ngôn ngữ sang tiếng việt : localhost:15536/vn/may-tinh-hp-phien-ban-dep





        /// <summary>
        /// OK, việc loại bỏ phân đoạn ngôn ngữ khỏi AppRelativeCurrentExecutionFilePath đã đc tiến hành 1 lần duy nhất trong BeginRequest. 
        /// Tất cả code làm việc dư thừa đã bị loại bỏ
        /// 
        /// 
        /// Hàm sẽ kiểm tra, nếu hệ thống được cấu hình dùng link dạng có phân đoạn language thì sẽ loại bỏ phân đoạn này khỏi 
        /// AppRelativeCurrentExecutionFilePath, để cho AppRelativeCurrentExecutionFilePath luôn luôn là dạng ko có phân đoạn ngôn ngữ
        /// những RawPath sẽ vẫn sẽ mang đầy đủ phân đoạn ngôn ngữ
        /// Nếu hệ thống ko dùng phân đoạn thì hàm chỉ đơn giản trả về hàm base.GetRouteData().
        /// </summary>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            //// nếu cấu hình link có phân đoạn ngôn ngữ đc bật ?
            //if(DataSettingsHelper.DatabaseIsInstalled() && this.SeoFriendlyUrlsForLanguagesEnabled)
            //{
            //    var request = httpContext.Request;
            //    // AppRelativeCurrentExecutionFilePath sẽ luôn trả về đường dẫn đúng cho dù web đang chạy trên 1 thư mục ảo, chẳng hạn
            //    // web đang chạy trên thư mục ảo /vietdung/ . Khi đó RawUrl sẽ có dạng /vietdung/us/login, nhưng 
            //    // AppRelativeCurrentExecutionFilePath sẽ luôn có dạng đúng ~/us/login
            //    string virtualPath = request.AppRelativeCurrentExecutionFilePath;
            //    string applicationPath = request.ApplicationPath; // kết quả thường là "/" đối với thư mục gốc, và sẽ có dạng /vietdung nếu nó chạy trên 1 thư mục ảo ?

            //    if (virtualPath.IsLocalizedUrl(applicationPath, false)) // nếu url có 1 phân đoạn 2 ký tự "giống" phân đoạn ngôn ngữ
            //    {
            //        //In ASP.NET Development Server, an URL like "http://localhost/Blog.aspx/Categories/BabyFrog" will return 
            //        //"~/Blog.aspx/Categories/BabyFrog" as AppRelativeCurrentExecutionFilePath.
            //        //However, in II6, the AppRelativeCurrentExecutionFilePath is "~/Blog.aspx"
            //        //It seems that IIS6 think we're process Blog.aspx page.
            //        //So, I'll use RawUrl to re-create an AppRelativeCurrentExecutionFilePath like ASP.NET Development Server.

            //        //Question: should we do path rewriting right here?
            //        // đoạn code này tù bỏ giá trị AppRelativeCurrentExecutionFilePath để lấy url từ RawPath với mục đích tương thích
            //        // với IIS 6 ><
            //        string rawUrl = request.RawUrl;
            //        var newVirtualPath = rawUrl.RemoveLanguageSeoCodeFromRawUrl(applicationPath);
            //        if (string.IsNullOrEmpty(newVirtualPath)) newVirtualPath = "/";
            //        newVirtualPath = newVirtualPath.RemoveApplicationPathFromRawUrl(applicationPath);
            //        newVirtualPath = "~" + newVirtualPath;
            //        httpContext.RewritePath(newVirtualPath, true);
            //        // RewritePath sẽ chỉ làm thay đổi kết quả trả về của request.AppRelativeCurrentExecutionFilePath, từ dạng
            //        // ~/en/xe-o-to thành dạng ~/xe-o-to, còn request.RawUrl thì vẫn giữ nguyên là /en/xe-o-to ko hề thay đổi
            //        // cho nên, những thao tác routing vốn dựa vào AppRelativeCurrentExecutionFilePath sẽ hoạt động như thể
            //        // ko hề có phân đoạn ngôn ngữ ở đây, còn thông qua RawUrl ta vẫn sẽ lấy ra đc phân đoạn ngôn ngữ và dùng 
            //        // nó theo mục đích của chúng ta

                    

                    
            //        // có 1 thảm họa ở đây:
            //        // đó là khi mà hệ thống hỗ trợ link có phân đoạn ngôn ngữ, khi đó đoạn RewritePath và cả thao tác RewritePath sẽ được thực hiện
            //        // n lần, với n là số routing thuộc họ LocalizedRoute tham gia vào phân giải url đến. Nhưng thao tác này ko quá đăt đỏ 
            //        // nhưng thừa thãi 1 cách ko cần thiết. Có thể xây dựng 1 interface IChuanHoaRouting, single per lifetimescopre, và dựa trên
            //        // per request cache. Ở đó ta sẽ phân tích Url 1 lần duy nhất ( lazy load ) + xác định luôn ngôn ngữ được sử dụng +
            //        // redirect nếu ngôn ngữ sai + Rewirite AppRelativeCurrentExecutionFilePath
            //    }
            //}
           

            var result = base.GetRouteData(httpContext);
            
            // nếu phân giải thành công, tiến hành kiểm tra sự tổn tại của phân đoạn ngôn ngữ, sự đúng đắn của phân đoạn ngôn ngữ
            // và redirect nếu thấy sai
            if (CheckSeoUrl) return CheckSeoFriendlyUrls(httpContext, result);
            return result;
        }

        protected virtual RouteData CheckSeoFriendlyUrls(HttpContextBase httpContext, 
            RouteData routeData, IWorkContext workContext = null)
        {
            if (routeData != null && DataSettingsHelper.DatabaseIsInstalled() && this.SeoFriendlyUrlsForLanguagesEnabled)
            {
                // lấy phân đoạn ngôn ngữ ra và kiểm tra
                var request = httpContext.Request;
                string rawUrl = request.RawUrl;
                string applicationPath = request.ApplicationPath;
                if(workContext == null)
                {
                    var engine = EngineContext.Current;
                    workContext = engine.Resolve<IWorkContext>();
                }
                
                var workingLanguage = workContext.WorkingLanguage;
                string redirectUrl = null;
                if (rawUrl.IsLocalizedUrl(applicationPath, true))
                {
                    string seoCode = rawUrl.GetLanguageSeoCodeFromUrl(applicationPath, true);
                    // phân đoạn ngôn ngữ không đúng với ngôn ngữ hiện hành, thay thế phân đoạn đúng và redirect ngay và luôn
                    if (!string.Equals(seoCode, workingLanguage.UniqueSeoCode, StringComparison.InvariantCultureIgnoreCase))
                        redirectUrl = rawUrl.ReplaceLanguageSeoCodeToRawUrl(applicationPath, workingLanguage);
                }
                else
                {
                    // ko có phân đoạn ngôn ngữ ? Thêm phân đoạn ngôn ngữ và redirect ngay và luôn
                    redirectUrl = rawUrl.AddLanguageSeoCodeToRawUrl(applicationPath, workingLanguage);
                }
                if (redirectUrl != null)
                {
                    RedirectToUrl(httpContext, redirectUrl, "302 Moved Temporarily");
                    return null;
                }
            }
            return routeData;
        }

        protected virtual void RedirectToUrl(HttpContextBase httpContext, string url, string statusString)
        {
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            var response = httpContext.Response;
            response.Status = statusString;
            response.RedirectLocation = webHelper.GetStoreLocation() +
                (url.StartsWith("/") ? url.Substring(1) : url);
            response.End(); // end() thế này là ok, cơ chế Routing đủ thông minh để nhận ra response đã kết thúc, và sẽ
            // break ngay lập tức công việc của mình để trả về chỉ thịc Redirect, do đó cũng sẽ ko có mối lo gì về
            // việc những thành phần khác của MVC đc thực thi phía sau
        }

        /// <summary>
        /// Override lại để add thêm phân đoạn ngôn ngữ vào phần đầu của đường dẫn trả về nếu cần thiết
        /// </summary>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            var result = base.GetVirtualPath(requestContext, values);

            if (result != null && DataSettingsHelper.DatabaseIsInstalled() && this.SeoFriendlyUrlsForLanguagesEnabled)
            {
                // nếu sinh link thành công và link là có hỗ trợ phân đoạn ngôn ngữ thì ta sẽ chèn phân đoạn ngôn ngữ vào kết quả
                // trước khi trả về
                var request = requestContext.HttpContext.Request;
                string rawUrl = request.RawUrl;
                string applicationPath = request.ApplicationPath;
                // nếu rawUrl là link có phân đoạn ngôn ngữ thì nối thêm phân đoạn ngôn ngữ vào phía trước chuỗi url kết quả
                if (rawUrl.IsLocalizedUrl(applicationPath, true))
                    result.VirtualPath = string.Concat(rawUrl.GetLanguageSeoCodeFromUrl(applicationPath, true), "/", result.VirtualPath);


                ////var workContext = EngineContext.Current.Resolve<IWorkContext>();
                ////result.VirtualPath = string.Concat(workContext.WorkingLanguage.UniqueSeoCode, "/", result.VirtualPath);
            }
            return result;
        }

        /// <summary>
        /// Hàm clear giá trị của _seoFriendlyUrlsForLanguagesEnabled về null để lần lazy loading tiếp theo lấy về giá trị mới cho
        /// _seoFriendlyUrlsForLanguagesEnabled. Nếu ko làm thế này, sẽ ko thể nào thay đổi được giá trị này khi mà setting của 
        /// LocalizationSettings thay đổi
        /// 
        /// Hàm sẽ được gọi như là hệ quả của thao tác clear cache trên bảng setting
        /// </summary>
        public virtual void ClearSeoFriendlyUrlsCachedValue()
        {
            _seoFriendlyUrlsForLanguagesEnabled = null;
        }

        #endregion
    }
}
