using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Cms;
using Research.Core.Plugins;
using Research.Core.Interface.Service.Cms;

namespace Research.Services.Cms
{
    public partial class WidgetService : IWidgetService
    {
        #region Fields, properties, and Ctors

        private readonly IPluginFinder _pluginFinder;
        private readonly WidgetSettings _widgetSettings;

        public WidgetService(IPluginFinder pluginFinder, WidgetSettings widgetSettings)
        {
            _pluginFinder = pluginFinder;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// nên kiểm tra p.PluginDescriptor.Installed == true, nhưng ở đây lại kiểm tra thông qua việc check sự tồn tại của SystemName
        /// trong WidgetSettings.ActiveWidgetSystemNames, điều này khiến cho việc câu hình Installed của các plugin thông qua file
        /// Description bị bỏ qua, và 1 widget plugin được coi là active hay ko chỉ dựa vào WidgetSettings.ActiveWidgetSystemNames mà
        /// không thực sự dựa vào cấu hình trong description.txt của chính nó
        /// 
        /// Ko phải, ý tưởng ở đây là cho phép disable widget ko hiển thị lên giao diện người dùng, nhưng lại ko disable plugin. Để
        /// làm được điều đó sẽ cần phải có 1 mục cấu hình riêng
        /// </summary>

        public virtual IList<IWidgetPlugin> LoadActiveWidgets(int storeId = 0)
        {
            return LoadActiveWidgetsShared(storeId).ToList();
        }

        protected virtual IEnumerable<IWidgetPlugin> LoadActiveWidgetsShared(int storeId = 0)
        {
            return LoadAllWidgetsShared(storeId).Where(p => _widgetSettings.ActiveWidgetSystemNames
                .Contains(p.PluginDescriptor.SystemName, StringComparer.InvariantCultureIgnoreCase));
        }

        public virtual IList<IWidgetPlugin> LoadActiveWidgetsByWidgetZone(string widgetZone, int storeId = 0)
        {
            if (string.IsNullOrWhiteSpace(widgetZone)) return new List<IWidgetPlugin>();
            return LoadActiveWidgetsShared(storeId)
                .Where(p => p.GetWidgetZones().Contains(widgetZone, StringComparer.InvariantCultureIgnoreCase))
                .ToList();
        }

        public virtual IWidgetPlugin LoadWidgetBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName)) return null;
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IWidgetPlugin>(systemName);
            return descriptor != null ? descriptor.Instance<IWidgetPlugin>() : null;
        }

        protected virtual IEnumerable<IWidgetPlugin> LoadAllWidgetsShared(int storeId = 0)
        {
            return _pluginFinder.GetPlugins<IWidgetPlugin>(storeId: storeId);
        }

        // sẽ chỉ lấy tất cả các widget plugin thuộc loại installed
        public virtual IList<IWidgetPlugin> LoadAllWidgets(int storeId = 0)
        {
            return LoadAllWidgetsShared(storeId).ToList();
        }

        #endregion
    }
}
