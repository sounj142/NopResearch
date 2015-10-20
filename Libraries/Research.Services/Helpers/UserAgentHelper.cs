using Research.Core;
using Research.Core.Common;
using Research.Core.Configuration;
using Research.Core.Infrastructure;
using Research.Core.Interface.Service;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web;
using UserAgentStringLibrary;

namespace Research.Services.Helpers
{
    public partial class UserAgentHelper : IUserAgentHelper
    {
        private readonly HttpContextBase _httpContext;

        public UserAgentHelper(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        
        protected static object _lockUasParser = new object();

        // ghi chú: Hàm này được viết khác với phiên bản của Nop.
        // Chúng ta hoàn toàn có thể sử dụng Autofac để duy trì 1 singleton cho UasParser. Cách dùng Singleton<UasParser>.Instance
        // có chăng chỉ giúp chạy nhanh hơn thôi ( hiện tại là thế, nếu có lợi ích gì hơn thì sẽ bổ sung sau )
        // thay vì lock bằng MethodImpl ( sai ), ta sẽ lock bằng 1 static key
        protected static UasParser GetUasParser()
        {
            if (Singleton<UasParser>.Instance == null)
            {
                var config = EngineContext.Current.Resolve<NopConfig>(); // lấy ra đối tượng nopConfig singleton
                if (string.IsNullOrEmpty(config.UserAgentStringsPath)) return null;
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();

                lock (_lockUasParser)
                {
                    if (Singleton<UasParser>.Instance == null)
                    {
                        string filePath = webHelper.MapPath(config.UserAgentStringsPath);
                        var uasParser = new UasParser(filePath);
                        Singleton<UasParser>.Instance = uasParser;
                    }
                }
            }
            return Singleton<UasParser>.Instance;
        }

        public bool IsSearchEngine()
        {
            if (_httpContext == null || _httpContext.Request == null) return false;
            var userAgent = _httpContext.Request.UserAgent;
            if (string.IsNullOrEmpty(userAgent)) return false;

            var uasParser = GetUasParser();
            if (uasParser == null) return false;

            // kiểm tra chuỗi userAgent trên 1 CSDL đã có để xác định xem đó có phải là userAgent do 1 máy tìm kiếm tạo ra hay ko
            return uasParser.IsBot(userAgent);

            // return context.Request.Browser.Crawler; // 1 cách khác ko tốt bằng ?
        }
    }
}
