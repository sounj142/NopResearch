
namespace Research.Core.Plugins
{
    /// <summary>
    /// Interface denoting plug-in attributes that are displayed throughout 
    /// the editing interface.
    /// 
    /// Giao diện cho các plugin của hệ thống nop ????
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Get hoặc set miêu tả/cấu hình của plugin. Việc thiết lập giá trị cho property này được thực hiện trong hàm 
        /// PluginDescriptor.Instance()
        /// </summary>
        PluginDescriptor PluginDescriptor { get; set; }

        /// <summary>
        /// Hàm cài đặt của plugin. Sau khi load hết các assemblies thì hệ thống sẽ lặp qua các plugin và triệu gọi hàm này để cài đặt nó
        /// . Hàm chứa các logic khởi tạo cần thiết cho plugin
        /// </summary>
        void Install();

        /// <summary>
        /// Hàm cho phép gỡ bỏ plugin khi ko không dùng đến ???
        /// </summary>
        void Uninstall();
    }
}
