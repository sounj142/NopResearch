using System;
using System.Web;
using System.Web.Routing;
using Research.Core;
using Research.Core.Data;
using Research.Core.Infrastructure;
using Research.Services.Events;
using Research.Services.Seo;
using Research.Web.Framework.Localization;
using Research.Core.Interface.Service;

namespace Research.Web.Framework.Seo
{
    /// <summary>
    /// Định nghĩa Route 1 phân đoạn của Nop, cho phép lấy ra các mẫu Url đa ngôn ngữ từ bảng UrlRecord, tuy nhiên lớp này có hạn chế nghiêm trọng
    /// là chỉ hỗ trợ được mấu có 1 phân đoạn ( phân đoạn ngôn ngữ nếu có đã được cắt bỏ bởi hàm trong lớp base LocalizedRoute )
    /// </summary>
    public partial class GenericPathRoute : LocalizedRoute
    {
        #region Fields, Properties, Ctors

        /// <summary>
        /// Override lại để ngăn chặn base.GetRouteData ko tự check phân đoạn ngôn ngữ quá sớm
        /// </summary>
        protected override bool CheckSeoUrl
        {
            get { return false; }
        }

        public GenericPathRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler)
        {
        }

        public GenericPathRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler)
        {
        }

        public GenericPathRoute(string url, RouteValueDictionary defaults,
            RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler)
        {
        }

        public GenericPathRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints,
            RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Phân giải Url tới. Trường hợp url có phân đoạn ngôn ngữ đã đc giải quyết bởi hàm base(). Hàm base() này loại bỏ phân đoạn ngôn ngữ khỏi
        /// AppRelativeCurrentExecutionFilePath, nên trong mọi trường hợp, Url mà hệ thống routing xử lý luôn là Url bình thường ko ngôn ngữ
        /// </summary>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var data = base.GetRouteData(httpContext); // vấn đề phân đoạn ngôn ngữ đã đc giải quyết ở đây
            // nếu url là dạng 1 phân đoạn thì nó sẽ đc phân giải thành công với generic_se_name là chuỗi phân đoạn này
            if (data != null && DataSettingsHelper.DatabaseIsInstalled())
            {
                var engine = EngineContext.Current;
                var urlRecordService = engine.Resolve<IUrlRecordService>();
                string slug = data.Values["generic_se_name"] as string;
                // urlRecord được tìm thông qua tìm kiếm nhị phân trên dữ liệu được cache hoặc 0(1) nếu cache riêng lwr từng slug
                var urlRecord = urlRecordService.GetBySlugCached(slug);
                if (urlRecord == null)
                {
                    data.Values["controller"] = "Common";
                    data.Values["action"] = "PageNotFound";
                    return data;
                }

                var workContext = engine.Resolve<IWorkContext>();
                var workingLanguage = workContext.WorkingLanguage;
                if (!urlRecord.IsActive)
                {
                    // vì đây là url cũ đã bị loại bỏ nên ta sẽ tìm url record tương ứng đang active và redirect qua đó
                    var activeSlug = SeoExtensions.GetSeName(urlRecord.EntityId, urlRecord.EntityName,
                        workingLanguage.Id, urlRecordService);
                    if (string.IsNullOrEmpty(activeSlug))
                    {
                        data.Values["controller"] = "Common";
                        data.Values["action"] = "PageNotFound";
                        return data;
                    }

                    // tìm thấy 1 url dạng active thay thế cho url "out of date" này

                    string newUrl = this.SeoFriendlyUrlsForLanguagesEnabled ?
                        workingLanguage.UniqueSeoCode + "/" + activeSlug 
                        : activeSlug;
                    RedirectToUrl(httpContext, newUrl, "301 Moved Permanently");
                    return null;

                    // vì GetStoreLocation() ko có phân đoạn ngôn ngữ nên quá trình redirect khi sai ngôn ngữ xảy ra như sau
                    // Nếu ngôn ngữ hiện hành là tiếng việt và link đúng là http://localhost:15536/vn/may-tinh-hp-phien-ban-dep
                    // 1. Truy cập bằng link http://localhost:15536/hp-pavilion-artist-edition-dv2890nr-141-inch-laptop
                    // 2. Sẽ bị 302 redirect qua http://localhost:15536/may-tinh-hp-phien-ban-dep ( redirect này được xử lý ở đoạn code
                    // bên dưới )
                    // 3. Sau đó lại bị 301 redirect lần nữa qua /vn/may-tinh-hp-phien-ban-dep ( ai thực hiện cái này thì ko biết )
                    // => Tức là hệ thống redirect tới 2 lần, ko hiệu quả. Cân xem xét lại để phối hợp thêm thông tin ngôn ngữ để
                    // chỉ cần redirect 1 lần, tức là cần viết thêm 1 hàm GetStoreLocationWithLanguageSegment() ???
                }

                // ok, urlRecord là active
                // cần đảm bảo rằng slug này là đúng với ngôn ngữ hiện hành, nếu ko đúng thì ta sẽ redirect qua ngôn ngữ hiện hành
                // VD như nếu ngôn ngữ hiện hành là tiếng việt và link là /computer thì ta sẽ redirect qua /may-vi-tinh
                //        Nếu link hiện hành là /vn/computer thì ta cũng redirect qua /vn/may-vi-tinh cho đúng với phân đoạn ngôn ngữ
                // 1 điểm đặc biệt của cơ chế routing này là nó cho phép ta kết hợp tùy ý giữa phân đoạn ngôn ngữ và phân đoạn seo url
                // mà vẫn có thể redirect đến link đúng


                // việc lấy slug theo ngôn ngữ hiện hành là cần thiết vì trong trường hợp chỉ có 1 ngôn ngữ languageid=1 active chẳng hạn, hệ thống sẽ tự động bỏ qua
                // url slug theo languageid=1 đó để chọn slug theo ngôn ngữ mặc định languageid=0 cho dù chúng ta tìm được urlRecord đúng
                // và thâm chí urlRecord.LanguageId == workContext.WorkingLanguage.Id
                var slugForCurrentLanguage = SeoExtensions.GetSeName(urlRecord.EntityId, urlRecord.EntityName,
                    workingLanguage.Id, urlRecordService);
                if (!string.IsNullOrEmpty(slugForCurrentLanguage) &&
                    !string.Equals(slug, slugForCurrentLanguage, StringComparison.InvariantCultureIgnoreCase))
                {
                    // nếu phát hiện ra slug ko đúng ngôn ngữ hiện hành, redirect sang link mới
                    //we should make not null or "" validation above because some entities does not have SeName for standard (ID=0) language (e.g. news, blog posts)

                    string newUrl = this.SeoFriendlyUrlsForLanguagesEnabled ?
                        workingLanguage.UniqueSeoCode + "/" + slugForCurrentLanguage
                        : slugForCurrentLanguage;
                    RedirectToUrl(httpContext, newUrl, "302 Moved Temporarily");
                    return null;
                }

                // ok, kiểm tra sự tồn tại và đúng của phân đoạn ngôn ngữ
                if (CheckSeoFriendlyUrls(httpContext, data, workContext) == null)
                    return null;

                // ok, đúng slug, đúng urlRecord, workContext.WorkingLanguage.Id là ngôn ngữ hiện hành
                // Bây giờ ta sẽ dựa trên giá trị của bộ 3 key để trả về controller/action/id phù hợp
                switch (urlRecord.EntityName)
                {
                    case "Product":
                        data.Values["controller"] = "Product";
                        data.Values["action"] = "ProductDetails";
                        data.Values["productid"] = urlRecord.EntityId;
                        data.Values["SeName"] = urlRecord.Slug;
                        break;
                    case "Category":
                        data.Values["controller"] = "Catalog";
                        data.Values["action"] = "Category";
                        data.Values["categoryid"] = urlRecord.EntityId;
                        data.Values["SeName"] = urlRecord.Slug;
                        break;
                    case "Manufacturer":
                        data.Values["controller"] = "Catalog";
                        data.Values["action"] = "Manufacturer";
                        data.Values["manufacturerid"] = urlRecord.EntityId;
                        data.Values["SeName"] = urlRecord.Slug;
                        break;
                    case "Vendor":
                        data.Values["controller"] = "Catalog";
                        data.Values["action"] = "Vendor";
                        data.Values["vendorid"] = urlRecord.EntityId;
                        data.Values["SeName"] = urlRecord.Slug;
                        break;
                    case "NewsItem":
                        data.Values["controller"] = "News";
                        data.Values["action"] = "NewsItem";
                        data.Values["newsItemId"] = urlRecord.EntityId;
                        data.Values["SeName"] = urlRecord.Slug;
                        break;
                    case "BlogPost":
                        data.Values["controller"] = "Blog";
                        data.Values["action"] = "BlogPost";
                        data.Values["blogPostId"] = urlRecord.EntityId;
                        data.Values["SeName"] = urlRecord.Slug;
                        break;
                    case "Topic":
                        data.Values["controller"] = "Topic";
                        data.Values["action"] = "TopicDetails";
                        data.Values["topicId"] = urlRecord.EntityId;
                        data.Values["SeName"] = urlRecord.Slug;
                        break;
                    default:
                        // urlRecord.EntityName là 1 "dạng lạ" mà ta chưa biết
                        // dùng bộ quản lý sự kiện IEventPublisher phát sinh 1 sự kiện để nhắc nhở nhà phát triển thêm vào code xử lý
                        // cho kiểu urlRecord.EntityName mới này ?
                        engine.Resolve<IEventPublisher>().Publish(new CustomUrlRecordEntityNameRequested(data, urlRecord));

                        // coi như là page not found
                        data.Values["controller"] = "Common";
                        data.Values["action"] = "PageNotFound";
                        break;
                }
            }
            return data;
        }

        #endregion
    }
}
