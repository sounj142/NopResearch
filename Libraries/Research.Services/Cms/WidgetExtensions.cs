using System;
using System.Linq;
using Research.Core.Domain.Cms;
using Research.Core.Interface.Service.Cms;
using Research.Core.Infrastructure;

namespace Research.Services.Cms
{
    public static class WidgetExtensions
    {
        public static bool IsWidgetActive(this IWidgetPlugin widget, WidgetSettings widgetSettings = null)
        {
            if (widgetSettings == null)
                widgetSettings = EngineContext.Current.Resolve<WidgetSettings>();
            return widgetSettings.ActiveWidgetSystemNames != null &&
                widgetSettings.ActiveWidgetSystemNames.Contains(widget.PluginDescriptor.SystemName,
                StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
