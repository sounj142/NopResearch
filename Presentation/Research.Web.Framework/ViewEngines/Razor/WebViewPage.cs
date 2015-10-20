using System;
using System.IO;
using System.Web.Mvc;
using System.Web.WebPages;
using Research.Core;
using Research.Core.Data;
using Research.Core.Infrastructure;
using Research.Web.Framework.Localization;
using Research.Web.Framework.Themes;
using Research.Core.Interface.Service;


namespace Research.Web.Framework.ViewEngines.Razor
{
    /// <summary>
    /// kế thừa từ WebViewPage để có thể render view với Razor. WebViewPage cũng chính là lớp được khai báo trong file web.config của 
    /// thư mục view.
    /// Đối tượng của lớp này sẽ được tạo ra mỗi khi render view. Vì chúng ta cần khai thác dữ liệu từ ILocalizationService, nhưng lại
    /// ko thể dùng DI để tạo ra đối tượng này khi cần trong View ( sẽ quá lãng phí hoặc tạo cú pháp ko đẹp ) => Ta sẽ khai báo
    /// tạo đối tượng này ngay trong hàm tạo của class WebViewPage
    /// </summary>
    public abstract class WebViewPage<TModel>: System.Web.Mvc.WebViewPage<TModel>
    {
        #region Field, Property, Ctor

        /// <summary>
        /// Service để lấy chuỗi resource
        /// </summary>
        private ILocalizationService _localizationService;

        /// <summary>
        /// delegate @T, chịu trách nhiệm lấy về chuỗi resource theo khóa tương ứng, với ngôn ngữ hiện hành từ _workContext.
        /// Trường hợp chuỗi resource dùng cú pháp giữ chỗ, cho phép format chuỗi với dãy tham số đi kèm
        /// </summary>
        private Localizer _localizer;

        /// <summary>
        /// Đối tượng ngữ cảnh công việc hiện hành
        /// </summary>
        private IWorkContext _workContext;

        /// <summary>
        /// chịu trách nhiệm lấy về chuỗi resource theo khóa tương ứng, với ngôn ngữ hiện hành từ _workContext.
        /// Trường hợp chuỗi resource dùng cú pháp giữ chỗ, cho phép format chuỗi với dãy tham số đi kèm
        /// </summary>
        public Localizer T
        {
            get
            {
                //// tạo mới _localizer theo lazy loading ?
                //if(_localizer == null)
                //{
                //    ////null localizer
                //    //_localizer = (resourceKey, args) => new LocalizedString(args == null || args.Length == 0 ?
                //    //    resourceKey : string.Format(resourceKey, args));

                //    //default localizer

                //}
                return _localizer; // ko dùng lazy loading vì thấy như thế giảm hiệu năng mà chả để làm gì
            }
        }

        /// <summary>
        /// Cho phép đối tượng ngữ cảnh làm việc có thể được lấy từ trong view theo cú pháp @WorkContext
        /// </summary>
        public IWorkContext WorkContext
        {
            get { return _workContext;  }
        }

        #endregion

        #region Method

        private LocalizedString GetLocalizedString(string resourceKey, params object[] args)
        {
            var str = _localizationService.GetResource(resourceKey, false);
            return new LocalizedString(args == null || args.Length == 0 ? str : string.Format(str, args));
        }

        public override void InitHelpers()
        {
            base.InitHelpers();
            if(DataSettingsHelper.DatabaseIsInstalled())
            {
                _localizer = GetLocalizedString;
                var enginer = EngineContext.Current;
                _localizationService = enginer.Resolve<ILocalizationService>();
                _workContext = enginer.Resolve<IWorkContext>();
            }
        }

        /// <summary>
        /// Cho phép render 1 thẻ section tự định nghĩa, kết quả sẽ là 1 thẻ div với các atrribute đi kèm, bên trong thẻ div này sẽ là 
        /// nội dung của section
        /// 
        /// 1 định nghĩa section bình thường sẽ đc render trực tiếp vào vị trí khai báo mà ko có gì bao quanh. 1 WrappedSection sẽ đc render
        /// vào trong 1 thẻ div với các html attribute tùy chọn cho thẻ div này
        /// </summary>
        /// <param name="name">Tên thẻ mới định nghĩa ?</param>
        /// <param name="wrapperHtmlAttributes">những thuộc tính html đi kèm</param>
        /// <returns>Trả về 1 helperResult bao quanh hàm định nghĩa WrappedSection</returns>
        public HelperResult RenderWrappedSection(string name, object wrapperHtmlAttributes)
        {
            Action<TextWriter> action = (tw) =>
                {
                    var section = RenderSection(name, false); // yêu cầu render section, nhưng ko bắt buộc phải có
                    if(section != null) // chỉ ghi kết quả vào TextWriter khi section tồn tại
                    {
                        // chuyển đổi những attribute dạng có dấu _ thành dạng dùng dấu - làm dấu nối
                        var htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(wrapperHtmlAttributes);
                        var tagBuilder = new TagBuilder("div"); // dùng tag div
                        tagBuilder.MergeAttributes(htmlAttributes); // đưa các atrribute vào thẻ div

                        // ghi 2 thẻ đóng mở div và chèn nội dung section vào giữa
                        tw.Write(tagBuilder.ToString(TagRenderMode.StartTag));
                        section.WriteTo(tw);
                        tw.Write(tagBuilder.ToString(TagRenderMode.EndTag));
                    }
                };
            return new HelperResult(action);
        }

        /// <summary>
        /// Hàm render section tự định nghĩa, gọi lại RenderSection của lớp cơ sở,  cho phép cung cấp nội dung mặc định khi render section
        /// </summary>
        public HelperResult RenderSection(string sectionName, Func<object, HelperResult> defaultContent)
        {
            // Nếu section sectionName được định nghĩa trong trang thì render, ngược lại dùng nội dung mặc định cung cấp bởi defaultContent
            return IsSectionDefined(sectionName) ? RenderSection(sectionName) : defaultContent(new object());
        }

        /// <summary>
        /// Mục đích của việc overide hàm get là để thay thế layout. Chẳng hạn như thế này:
        /// + Ta có 1 layout trong thư mục theme mặc định và 1 layout trong thư mục ~/View. Khi đó layout của theme sẽ được dùng thay thế
        /// cho layout của ~/View
        /// + Ta có 1 view ~/View/Home/Index.cshtml định nghĩa trong ~/View . Khi đó view này vẫn sẽ dùng layout trong theme dù rằng
        /// trong index view đó có 1 dòng khai báo rõ ràng rằng hãy dùng layout của ~/View
        /// 
        /// Ta đạt được điều này bằng cách override lại property Layout, ở đó ta sẽ hoàn toàn "lơ" luôn giá trị base.Layout, vốn trỏ
        /// tới layout của ~/View, và ta sẽ ưu tiên tìm view của theme và xài view theme để thay thế nếu có thể
        /// 
        /// Đây chính là lý do mà dù ta có thiết lập Layout cụ thể cho 1 View nào đó để nó xài layout mà chúng ta mong muốn thì hệ thống
        /// cũng sẽ chạy đi chọn layout mặc định cùng tên ở trong thư mục Theme hoặc ~/View để xài, bất kể nỗ lực của chúng ta
        /// 
        /// => Giải pháp : Đặt tên cho layout quái dị 1 chút, sao cho nó khác tên layout dùng cho các theme hoặc ~/View
        /// 1 Giải pháp nữa là thêm 1 property bool DoNotAutoOverrideLayout để qui định rằng ko đc phép tự ý thiết lập lại layout.
        /// Khi đó cần sửa lại property Layout để cân nhắc đến giá trị bool này
        /// </summary>
        public override string Layout
        {
            get
            {
                string layout = base.Layout;
                if(!string.IsNullOrEmpty(layout)) 
                {
                    string fileName = Path.GetFileNameWithoutExtension(layout);
                    var viewResult = System.Web.Mvc.ViewEngines.Engines.FindView(ViewContext.Controller.ControllerContext, fileName, "");
                    var razorView = viewResult.View as RazorView;
                    if (razorView != null && !string.IsNullOrEmpty(razorView.ViewPath)) layout = razorView.ViewPath;
                }
                return layout;
            }
            set
            {
                base.Layout = value; // ko thay đổi gì nếu bên trong View chỉ định layout 1 cách rõ ràng bằng lệnh gán Layout = .... ;
            }
        }

        /// <summary>
        /// Return a value indicating whether the working language and theme support RTL (right-to-left)
        /// </summary>
        public bool ShouldUseRtlTheme()
        {
            bool supportRtl = _workContext.WorkingLanguage.Rtl; // ngôn ngữ hiện hành là loại hiển thị phải qua trái ?
            if (supportRtl)
            {
                // kiểm tra để đảm bảo là theme hiện hành cũng hỗ trợ điều đó
                var enginer = EngineContext.Current;
                var themeProvider = enginer.Resolve<IThemeProvider>();
                var themeContext = enginer.Resolve<IThemeContext>();
                supportRtl = themeProvider.GetThemeConfiguration(themeContext.WorkingThemeName).SupportRtl;
            }
            return supportRtl;
        }

        /// <summary>
        /// Gets a selected tab index (used in admin area to store selected tab index)
        /// Lưu trữ tab được chọn để bôi sáng trong menu admin ?
        /// </summary>
        /// <returns>Index</returns>
        public int GetSelectedTabIndex()
        {
            //keep this method synchornized with
            //"SetSelectedTabIndex" method of \Administration\Controllers\BaseNopController.cs
            int index = 0;
            const string dataKey = "nop.selected-tab-index";

            object obj = TempData[dataKey];
            if (obj is int) index = (int)obj;
            else
            {
                obj = ViewData[dataKey];
                if (obj is int) index = (int)obj;
            }
            if (index < 0) index = 0;
            return index;
        }

        #endregion
    }

    public abstract class WebViewPage: WebViewPage<dynamic>
    {
    }
}
