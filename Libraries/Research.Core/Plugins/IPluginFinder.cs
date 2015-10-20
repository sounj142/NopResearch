using System.Collections.Generic;

namespace Research.Core.Plugins
{
    /// <summary>
    /// Plugin finder
    /// Giao diện đóng vai trò bộ tìm kiếm plugin, cho phép tìm kiếm IPlugin, PluginDescriptor theo nhiều tiêu chí  ???
    /// </summary>
    public interface IPluginFinder
    {
        /// <summary>
        /// Kiểm tra xem plugin có hỗ trợ cho store storeId hay ko ( dựa vào thông tin trong PluginDescriptor.LimitedToStores )
        /// </summary>
        /// <param name="pluginDescriptor">Plugin descriptor to check</param>
        /// <param name="storeId">Store identifier to check</param>
        /// <returns>true - available; false - no</returns>
        bool AuthenticateStore(PluginDescriptor pluginDescriptor, int storeId);


        /// <summary>
        /// Lấy về tất cả các nhóm plugin ( PluginDescriptor.Group )
        /// </summary>
        IEnumerable<string> GetPluginGroups();

        /// <summary>
        /// Hàm lấy về tất cả các plugin thỏa mãn các điều kiện tìm kiếm cho trước
        /// </summary>
        /// <typeparam name="T">The type of plugins to get.</typeparam>
        /// <param name="loadMode">Load plugins mode</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <param name="group">Filter by plugin group; pass null to load all records</param>
        /// <returns>Plugins</returns>
        IEnumerable<T> GetPlugins<T>(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, int storeId = 0,
            string group = null) where T : class, IPlugin;

        /// <summary>
        /// Lấy về các plugin descriptor theo các điều kiện tìm kiếm cho trước
        /// </summary>
        /// <param name="loadMode">Load plugins mode</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <param name="group">Filter by plugin group; pass null to load all records</param>
        /// <returns>Plugin descriptors</returns>
        IEnumerable<PluginDescriptor> GetPluginDescriptors(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
            int storeId = 0, string group = null);


        /// <summary>
        /// Lấy về các plugin descriptor theo các điều kiện tìm kiếm cho trước, với T là kiểu của plugin cần lấy ( PluginDescriptor.PluginType )
        /// </summary>
        /// <typeparam name="T">The type of plugin to get.</typeparam>
        /// <param name="loadMode">Load plugins mode</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <param name="group">Filter by plugin group; pass null to load all records</param>
        /// <returns>Plugin descriptors</returns>
        IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly,
            int storeId = 0, string group = null) where T : class, IPlugin;

        /// <summary>
        /// Lấy về PluginDescriptor theo tên nhận diện systemName
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        /// <param name="loadMode">Load plugins mode</param>
        /// <returns>>Plugin descriptor</returns>
        PluginDescriptor GetPluginDescriptorBySystemName(string systemName, LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly);

        /// <summary>
        /// Lấy về PluginDescriptor theo tên nhận diện systemName, trong đó kiểu của plugin phải là T hoặc kiểu kế thừa từ T
        /// </summary>
        /// <typeparam name="T">The type of plugin to get.</typeparam>
        /// <param name="systemName">Plugin system name</param>
        /// <param name="loadMode">Load plugins mode</param>
        /// <returns>>Plugin descriptor</returns>
        PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
            where T : class, IPlugin;

        /// <summary>
        /// Reload plugins
        /// Reload tất cả các plugin trong bộ tìm kiếm ( nói chung là reload lại bộ nhớ cache của bộ tìm kiếm để nhận diện lại plugin,
        /// giúp phép tìm kiếm ko bị out of date ) ????
        /// </summary>
        void ReloadPlugins();
    }
}
