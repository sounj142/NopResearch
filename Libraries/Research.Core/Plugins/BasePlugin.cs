

namespace Research.Core.Plugins
{
    /// <summary>
    /// Lớp cơ sở để tất cả các plugin của hệ thống Nop kế thừa. Mỗi plugin của hệ thống nop có thể có các định nghĩa class, controller,
    /// view của riêng nó, nhưng phải khai báo 1 lớp kế thừa BasePlugin, trong đó có các logic cần thiết để khai báo, install, uninstall
    /// plugin
    /// </summary>
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        /// Get hoặc set miêu tả/cấu hình của plugin
        /// </summary>
        public virtual PluginDescriptor PluginDescriptor { get; set; }

        /// <summary>
        /// Hàm cài đặt của plugin. Sau khi load hết các assemblies thì hệ thống sẽ lặp qua các plugin và triệu gọi hàm này để cài đặt nó
        /// . Hàm chứa các logic khởi tạo cần thiết cho plugin
        /// </summary>
        public virtual void Install()
        {
            // 1 thao tác quan trọng cần làm khi gọi hàm Install() của 1 plugin đó là cần phải đánh dấu nó là đã được Install vào file
            // InstallerPlugins.txt
            PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName); // ko đánh dấu PluginDescriptor.Install = true ?
        }

        /// <summary>
        /// Hàm cho phép gỡ bỏ plugin khi ko không dùng đến ???
        /// </summary>
        public virtual void Uninstall()
        {
            PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
        }
    }
}
