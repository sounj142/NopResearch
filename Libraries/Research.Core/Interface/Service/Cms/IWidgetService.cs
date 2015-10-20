using System.Collections.Generic;

namespace Research.Core.Interface.Service.Cms
{
    /// <summary>
    /// Widget service interface.
    /// Service lấy về danh sách các Widget đang có của hệ thống ( đọc assembly ), tìm tất cả các widget có yêu cầu render ra 1
    /// widget zone nào đó ,...v.v.v.
    /// </summary>
    public partial interface IWidgetService
    {
        /// <summary>
        /// Đọc assembly để lấy về danh sách các IWidgetPlugin đang active. Các widget active là các widget thuộc về các plugin 
        /// có property cấu hình Installed == true.
        /// 
        /// Ghi nhớ: Cấu hình giới hạn plugin theo Store được đặt trong description.txt của file cấu hình plugin
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        IList<IWidgetPlugin> LoadActiveWidgets(int storeId = 0);

        /// <summary>
        /// Cho phép lấy về tất cả các widget active nào có nhu cầu được render vào 1 zone cụ thể, ví dụ như khi gặp chỉ thị
        /// @Html.Widget("home_page_top"), chúng ta sẽ gọi đến IWidgetService.LoadActiveWidgetsByWidgetZone("home_page_top", storeId)
        /// để lấy về danh sách các widget có đăng ký render vào "home_page_top" zone. Sau đó, với mỗi IWidgetPlugin nhận được, lại
        /// gọi đến IWidgetPlugin.GetDisplayWidgetRoute() để lấy về danh sách các tham số để gọi render action method tương ứng
        /// </summary>
        IList<IWidgetPlugin> LoadActiveWidgetsByWidgetZone(string widgetZone, int storeId = 0);

        /// <summary>
        /// Lấy về widget theo tên systemName ???
        /// systemName chính là IPlugin.PluginDescriptor.SystemName, tức là tên SystemName của plugin, tên này là duy nhất cho mỗi
        /// plugin, trong khi đó LoadWidgetBySystemName khi nhận vào 1 systemName lại chỉ trả về tối đa 1 IWidgetPlugin, điều đó đồng
        /// nghĩa với trong 1 plugin sẽ chỉ có 1 đối tượng IWidgetPlugin duy nhất. Liệu chừng đó có đủ xài hay ko ?
        /// </summary>
        IWidgetPlugin LoadWidgetBySystemName(string systemName);

        /// <summary>
        /// khác với hàm LoadActiveWidgets, hàm này lấy về tất cả các widget của storeId, bất kể nó có active hay ko
        /// </summary>
        IList<IWidgetPlugin> LoadAllWidgets(int storeId = 0);
    }
}
