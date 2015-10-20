using System.Collections.Generic;
using Research.Core.Configuration;

namespace Research.Core.Domain.Cms
{
    /// <summary>
    /// Đối tượng cấu hình cho chức năng load widget từ các plugin
    /// </summary>
    public class WidgetSettings : ISettings
    {
        public WidgetSettings()
        {
            ActiveWidgetSystemNames = new List<string>();
        }

        /// <summary>
        /// Gets or sets a system names of active widgets.
        /// Danh sách các system name của các widget hiện đang active ??
        /// </summary>
        public List<string> ActiveWidgetSystemNames { get; set; }
    }
}
