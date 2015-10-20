using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Core.Plugins
{
    // khác với Nop, class PluginFinder đã được điều chình để co thể tồn tại singleton
    public class PluginFinder : IPluginFinder
    {
        #region Fields, Propperties, Ctors

        /// <summary>
        /// Danh sách các cấu hình plugin hiện đang có trong AppDomain
        /// </summary>
        private IList<PluginDescriptor> _plugins;

        /// <summary>
        /// Cờ đánh dấu cho biết dữ liệu đã được cung cấp cho _plugins hay chưa ?
        /// </summary>
        private bool _arePluginsLoaded;

        private readonly object _lockObject = new object();


        #endregion

        #region Utilities

        /// <summary>
        /// Ensure plugins are loaded
        /// Gọi hàm để đảm bảo rằng _plugins đã được cung cấp dữ liệu là tập hợp các plugin được load lên AppDomain
        /// </summary>
        protected virtual void EnsurePluginsAreLoaded()
        {
            if(!_arePluginsLoaded)
            {
                lock(_lockObject)
                {
                    if (!_arePluginsLoaded)
                    {
                        var foundPlugins = PluginManager.ReferencedPlugins.ToList();
                        foundPlugins.Sort();
                        _plugins = foundPlugins;

                        _arePluginsLoaded = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// Kiểm tra xem plugin pluginDescriptor có thể được load ở mode loadMode hay ko
        /// </summary>
        protected virtual bool CheckLoadMode(PluginDescriptor pluginDescriptor, LoadPluginsMode loadMode)
        {
            if (pluginDescriptor == null) throw new ArgumentNullException("pluginDescriptor"); 

            switch(loadMode)
            {
                case LoadPluginsMode.All:
                    return true;
                case LoadPluginsMode.InstalledOnly:
                    return pluginDescriptor.Installed;
                case LoadPluginsMode.NotInstalledOnly:
                    return !pluginDescriptor.Installed;
            }
            throw new Exception("Not supported LoadPluginsMode");
        }

        /// <summary>
        /// Kiểm tra xem pluginDescriptor có thuộc nhóm group hay ko ( nếu group = empty thì luôn trả về true )
        /// </summary>
        protected virtual bool CheckGroup(PluginDescriptor pluginDescriptor, string group)
        {
            if (pluginDescriptor == null) throw new ArgumentNullException("pluginDescriptor");

            return string.IsNullOrEmpty(group) ||
                string.Equals(pluginDescriptor.Group, group, StringComparison.InvariantCulture);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Kiểm tra xem plugin có hỗ trợ cho store storeId hay ko ( dựa vào thông tin trong PluginDescriptor.LimitedToStores )
        /// Sẽ luôn trả về true với storeId = 0
        /// </summary>
        public virtual bool AuthenticateStore(PluginDescriptor pluginDescriptor, int storeId)
        {
            if (pluginDescriptor == null) throw new ArgumentNullException("pluginDescriptor");

            return storeId == 0 || pluginDescriptor.LimitedToStores.Count == 0 || 
                pluginDescriptor.LimitedToStores.Contains(storeId);
        }

        /// <summary>
        /// Lấy về tất cả các nhóm plugin hiện có ( PluginDescriptor.Group )
        /// </summary>
        public virtual IEnumerable<string> GetPluginGroups()
        {
            return GetPluginDescriptors(LoadPluginsMode.All).Select(p => p.Group)
                .Distinct(StringComparer.InvariantCulture).OrderBy(p => p);
        }


        /// <summary>
        /// Hàm lấy về tất cả các plugin thỏa mãn các điều kiện tìm kiếm cho trước
        /// </summary>
        public virtual IEnumerable<T> GetPlugins<T>(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, 
            int storeId = 0, string group = null) where T : class, IPlugin
        {
            return GetPluginDescriptors<T>(loadMode, storeId, group).Select(p => p.Instance<T>());
        }

        /// <summary>
        /// Lấy về các plugin descriptor theo các điều kiện tìm kiếm cho trước
        /// </summary>
        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, 
            int storeId = 0, string group = null)
        {
            EnsurePluginsAreLoaded();
            return _plugins.Where(p => CheckLoadMode(p, loadMode) && CheckGroup(p, group) && AuthenticateStore(p, storeId));
        }

        /// <summary>
        /// Lấy về các plugin descriptor theo các điều kiện tìm kiếm cho trước, với T là kiểu của plugin cần lấy ( PluginDescriptor.PluginType )
        /// </summary>
        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, 
            int storeId = 0, string group = null) where T : class, IPlugin
        {
            var typeOfT = typeof(T);
            return GetPluginDescriptors(loadMode, storeId, group).Where(p => typeOfT.IsAssignableFrom(p.PluginType));
        }

        /// <summary>
        /// Lấy về PluginDescriptor theo tên nhận diện systemName
        /// </summary>
        public virtual PluginDescriptor GetPluginDescriptorBySystemName(string systemName,
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
        {
            if (string.IsNullOrEmpty(systemName)) return null;
            return GetPluginDescriptors(loadMode).FirstOrDefault(p =>
                string.Equals(p.SystemName, systemName, StringComparison.InvariantCulture));
        }

        public virtual PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, 
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly) where T : class, IPlugin
        {
            if (string.IsNullOrEmpty(systemName)) return null;
            return GetPluginDescriptors<T>(loadMode).FirstOrDefault(p =>
                string.Equals(p.SystemName, systemName, StringComparison.InvariantCulture));
        }

        public virtual void ReloadPlugins()
        {
            _arePluginsLoaded = false;
            EnsurePluginsAreLoaded();
        }

        #endregion
    }
}
