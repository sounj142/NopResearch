using System.Collections.Generic;
using System.Web.Routing;
using Research.Core.Plugins;

namespace Research.Core.Interface.Service.Cms
{
    /// <summary>
    /// Bên ngoài view có định nghĩa nhưng khối Widget, đây là vị trí mà các plugin của bên thứ 3 có thể điền nội dung của nó vào,
    /// giả dụ như có 3 nội dung đến từ các nguồn khác nhau được điền vào widget home_page_top, thì cả 3 nguồn nội dung đó đều phải cài
    /// đặt giao diện IPlugin để có thể được phát hiện động bởi hệ thống.
    /// 
    /// Ghi chú: Vì IWidgetPlugin cài đặt IPlugin nên nhìn chung là mỗi plugin sẽ chỉ có duy nhất 1 đối tượng IWidgetPlugin, và đây
    /// là điểm ra mà Nop dùng để nhận biết các nhu cầu về widget của plugin
    /// </summary>
    public partial interface IWidgetPlugin : IPlugin
    {
        /// <summary>
        /// Lấy về danh sách tên các vùng wisget mà IWidgetPlugin này muốn điền nội dung vào, chẳng hạn như đâu đó trong view có 1
        /// vùng nội dung có dạng @Html.Widget("home_page_top"). Để được điền vào vùng nội dung này thì đối tượng IWidgetPlugin phải
        /// được cài đặt với hàm GetWidgetZones trả về 1 phần tử có giá trị là "home_page_top"
        /// </summary>
        IList<string> GetWidgetZones();

        /// <summary>
        /// Gets a route for plugin configuration
        /// 
        /// trả về action/controller/routeData của action method cần được gọi khi cấu hình widget.
        /// Hàm này sẽ render về nguyên xi 1 trang đầy đủ ( có đầy đủ mọi thành phần layout và nội dung ) để dùng làm 1 trang quản trị
        /// tương ứng với chức năn gplugin trong phần admin ???
        /// </summary>
        void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);

        /// <summary>
        /// Gets a route for displaying widget. Trả về tên action/controler/routerData cần thiết để render ra widget mang tên widgetZone.
        /// IWidgetPlugin có thể render ra nhiều widgetZone khác nhau, và nội dung hiển thị ở mỗi widgetZone có thể khác nhau
        /// 
        /// Để có thể được render vào bởi 1 chỉ thị @Html.Widget("???"), đối tượng IWidgetPlugin phải trả về tên controller, tên 
        /// action và danh sách routeValue cho hàm render @Html.Widget() biết để có thể triệu gọi đúng. Bởi vì IWidgetPlugin được
        /// định nghĩa bởi bên thứ 3 nên @Html.Widget() ko thể biết IWidgetPlugin như thế nào khi nó gọi, giải pháp là IWidgetPlugin
        /// cung cấp đầy đủ thông tin cần thiết để  @Html.Widget() gọi, theo qui định cho bởi giao diện IWidgetPlugin
        /// </summary>
        void GetDisplayWidgetRoute(string widgetZone,out string actionName, out string controllerName, 
            out RouteValueDictionary routeValues);
    }
}
