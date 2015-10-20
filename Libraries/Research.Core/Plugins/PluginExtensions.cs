using System;
using System.IO;

namespace Research.Core.Plugins
{
    public static class PluginExtensions
    {
        /// <summary>
        /// Trả về đường dẫn file logo của plugin ( qui ước đó là file "logo.jpg" nằm trong thư mục gốc của plugin )
        /// </summary>
        public static string GetLogoUrl(this PluginDescriptor pluginDescriptor, IWebHelper webHelper)
        {
            if (pluginDescriptor == null) throw new ArgumentNullException("pluginDescriptor");
            if (webHelper == null) throw new ArgumentNullException("webHelper");
            if (pluginDescriptor.OriginalAssemblyFile == null || pluginDescriptor.OriginalAssemblyFile.Directory == null)
                return null;

            var pluginDirectory = pluginDescriptor.OriginalAssemblyFile.Directory;
            string logoLocalPath = Path.Combine(pluginDirectory.FullName, "logo.jpg");
            if(!File.Exists(logoLocalPath)) return null;

            return string.Format("{0}plugins/{1}/logo.jpg", webHelper.GetStoreLocation(), pluginDirectory.Name);
        }
    }
}
