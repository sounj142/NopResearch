using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Research.Core.Infrastructure;
using Research.Core.Common;
using Research.Core.Domain.Localization;
using Research.Core.Data;

namespace Research.Core
{
    public partial class WebHelper: IWebHelper
    {
        #region Fields, Properties, Ctors

        private static string _newForwardedHTTPheader;
        private static object _lock = new object();

        protected static string ForwardedHTTPheader
        {
            get
            {
                if (_newForwardedHTTPheader == null)
                {
                    lock (_lock) // sẽ chỉ khóa duy nhất 1 lần
                    {
                        if (_newForwardedHTTPheader == null)
                        {
                            _newForwardedHTTPheader = ConfigurationManager.AppSettings["ForwardedHTTPheader"];
                            if (string.IsNullOrWhiteSpace(_newForwardedHTTPheader)) _newForwardedHTTPheader = "X-FORWARDED-FOR";
                        }
                    }
                }
                return _newForwardedHTTPheader;
            }
        }

        private readonly HttpContextBase _httpContext;

        public WebHelper(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        #endregion

        #region Utilities
        /// <summary>
        /// Kiểm tra xem đối tượng httpContext có "xài đc" hay ko, nói chung là lmf công việc khá thừa thãi
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual bool IsRequestAvailable(HttpContextBase httpContext)
        {
            if (httpContext == null) return false;
            try
            {
                if (httpContext.Request != null) return true;
            }catch
            {
            }
            return false;
        }

        #endregion

        #region methods
        // trả về url referer gồm path và query ( ko bao gồm domain port )
        public virtual string GetUrlReferrer()
        {
            if (IsRequestAvailable(_httpContext) && _httpContext.Request.UrlReferrer != null)
                return _httpContext.Request.UrlReferrer.PathAndQuery;
            return string.Empty;
        }

        /// <summary>
        /// trả về IP Address của request
        /// </summary>
        public virtual string GetCurrentIpAddress()
        {
            if (!IsRequestAvailable(_httpContext)) return string.Empty;

            var result = string.Empty;
            if(_httpContext.Request.Headers != null)
            {
                //The X-Forwarded-For (XFF) HTTP header field is a de facto standard
                //for identifying the originating IP address of a client
                //connecting to a web server through an HTTP proxy or load balancer.

                //but in some cases server use other HTTP header
                //in these cases an administrator can specify a custom Forwarded HTTP header
                string forwardedHttpHeader = ForwardedHTTPheader;

                //it's used for identifying the originating IP address of a client connecting to a web server
                //through an HTTP proxy or load balancer. 
                string xff = _httpContext.Request.Headers.AllKeys
                    .Where(x => string.Equals(x, forwardedHttpHeader, StringComparison.InvariantCultureIgnoreCase))
                    .Select(k => _httpContext.Request.Headers[k])
                    .FirstOrDefault();

                //if you want to exclude private IP addresses, then see http://stackoverflow.com/questions/2577496/how-can-i-get-the-clients-ip-address-in-asp-net-mvc
                if(!string.IsNullOrWhiteSpace(xff))
                {
                    result = xff.Split(',').FirstOrDefault();
                }
            }

            if (string.IsNullOrWhiteSpace(result) && _httpContext.Request.UserHostAddress != null)
                result = _httpContext.Request.UserHostAddress;
            if (result == "::1") result = "127.0.0.1";
            if (!string.IsNullOrWhiteSpace(result))
            {
                int index = result.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
                if (index >= 0) result = result.Substring(0, index);
            }

            return result;
        }

        /// <summary>
        /// Gets this page name. Lấy về Url của request hiện hành
        /// </summary>
        /// <param name="includeQueryString">Value indicating whether to include query strings</param>
        /// <returns>Page name</returns>
        public virtual string GetThisPageUrl(bool includeQueryString)
        {
            return GetThisPageUrl(includeQueryString, IsCurrentConnectionSecured());
        }

        /// <summary>
        /// Lấy về Url của request hiện hành
        /// </summary>
        /// <param name="includeQueryString">Value indicating whether to include query strings</param>
        /// <param name="useSsl">Value indicating whether to get SSL protected page</param>
        /// <returns>Page name</returns>
        public virtual string GetThisPageUrl(bool includeQueryString, bool useSsl)
        {
            string url = string.Empty;
            if (!IsRequestAvailable(_httpContext)) return url;
            if (includeQueryString)
            {
                string storeHost = GetStoreHost(useSsl);
                if (storeHost.EndsWith("/"))
                    storeHost = storeHost.Substring(0, storeHost.Length - 1);
                url = storeHost + _httpContext.Request.RawUrl;
            }else
            {
                if (_httpContext.Request.Url != null)
                    url = _httpContext.Request.Url.GetLeftPart(UriPartial.Path);
            }
            return url.ToLowerInvariant();
        }

        /// <summary>
        /// Kiểm tra xem request hiện tại có phải SSL hay ko
        /// </summary>
        public virtual bool IsCurrentConnectionSecured()
        {
            if(IsRequestAvailable(_httpContext))
            {
                return _httpContext.Request.IsSecureConnection;
                //when your hosting uses a load balancer on their server then the Request.IsSecureConnection is never got set to true, use the statement below
                //just uncomment it
                //return _httpContext.Request.ServerVariables["HTTP_CLUSTER_HTTPS"] == "on" ? true : false;
            }
            return false;
        }

        /// <summary>
        /// Lấy biến Server theo tên
        /// </summary>
        public virtual string ServerVariables(string name)
        {
            string empty = string.Empty;
            try
            {
                if (!IsRequestAvailable(_httpContext)) return empty;
                return _httpContext.Request.ServerVariables[name] ?? empty;
            }catch
            {
                return empty;
            }
        }

        /// <summary>
        /// Lấy về đường dẫn host hiện hành, đường dẫn này tùy trường hợp được ưu tiên lấy về từ trong các property cấu hình Url, SecureUrl của
        /// current store hoặc lấy từ ServerVariables("HTTP_HOST")
        /// </summary>
        /// <param name="useSsl"></param>
        /// <returns></returns>
        public virtual string GetStoreHost(bool useSsl)
        {
            string result = string.Empty;
            string httpHost = ServerVariables("HTTP_HOST");
            if (!string.IsNullOrEmpty(httpHost))
                result = "http://" + httpHost; // xử lý khi httpHost != null :  lấy về host và gán vào result 

            if(DataSettingsHelper.DatabaseIsInstalled())
            {
                #region Database is installed

                var engine = EngineContext.Current;
                var storeContext = engine.Resolve<IStoreContext>();
                var currentStore = storeContext.CurrentStore;
                if (currentStore == null) throw new Exception("Store hiện hành không thể load");

                if(string.IsNullOrWhiteSpace(httpHost))
                {
                    // biến server HTTP_HOST ko tồn tại, tình huống này xảy ra khi HttpContext là fake, ví dụ như khi chạy trong 1 tác vụ
                    // Trong trường hợp này, dùng url của 1 store được cấu hình là store mặc định trong phần admin làm store host

                    result = currentStore.Url;
                }
                if(useSsl)
                {
                    if (!string.IsNullOrWhiteSpace(currentStore.SecureUrl))
                        result = currentStore.SecureUrl;
                    else
                    {
                        if (result.StartsWith("http:", StringComparison.InvariantCultureIgnoreCase))
                            result = "https:" + result.Substring(5);
                    }
                }
                else
                {
                    if (currentStore.SslEnabled && !string.IsNullOrWhiteSpace(currentStore.SecureUrl))
                        result = currentStore.Url;
                }

                #endregion
            }else
            {
                #region Database is not installed

                if (useSsl && result.StartsWith("http:", StringComparison.InvariantCultureIgnoreCase))
                        result = "https:" + result.Substring(5);

                #endregion
            }

            if (!result.EndsWith("/")) result += "/";
            return result.ToLowerInvariant();
        }

        public virtual string GetStoreLocation()
        {
            return GetStoreLocation(IsCurrentConnectionSecured());
        }

        // webHelper.GetStoreLocation(false) sẽ có cả phân đoạn ngôn ngữ ?
        // Trả lời: Sẽ ko có phân đoạn ngôn ngữ. GetStoreLocation sẽ trả về host + applicationPath, tức là đường dẫn đầy đủ của
        // ứng dụng tính đến thư mục gốc ( bao gồm cả http, domain, port, kết thúc bằng dấu / ), cho nên sẽ ko có thêm phân đoạn ngôn ngữ ở đây
        // => cần viết thêm 1 hàm cho phép trả về thêm phân đoạn ngôn ngữ, sẽ hữu dụng hơn ?
        public virtual string GetStoreLocation(bool useSsl)
        {
            string result = GetStoreHost(useSsl);
            if (result.EndsWith("/")) result = result.Substring(0, result.Length - 1);
            if (IsRequestAvailable(_httpContext))
                result += _httpContext.Request.ApplicationPath;
            if (!result.EndsWith("/")) result += "/";

            return result.ToLowerInvariant();
        }

        private static readonly string[] StaticResourceExtension = 
        {
            ".axd", ".ashx", ".bmp", ".css", ".gif", ".htm", ".html", ".ico", ".jpeg",
            ".jpg", ".js", ".png", ".rar" , ".zip" 
        };

        /// <summary>
        /// Trả về true nếu request yêu cầu 1 tài nguyên thuộc loại file( hình ảnh hoặc ashx ), 
        /// ko cần phải xử lý như thể là 1 request MVC( riêng aspx thì sẽ được hệ routing xử lý như thể là những link cũ, sẽ được điều
        /// hướng để redirect đến link mới )
        /// </summary>
        public virtual bool IsStaticResource(HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            string extension = VirtualPathUtility.GetExtension(request.Path);
            if (string.IsNullOrWhiteSpace(extension)) return false;
            return StaticResourceExtension.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Map 1 dường dẫn ảo thành đường dẫn vật lý, dùng được cho cả trường hợp chạy thực và unit test
        /// </summary>
        public virtual string MapPath(string path)
        {
            return WebCommon.MapPath(path);
        }

        public virtual string ModifyQueryString(string url, string queryStringModification, string anchor)
        {
            throw new NotImplementedException();
        }

        public virtual string RemoveQueryString(string url, string queryString)
        {
            throw new NotImplementedException();
        }

        public virtual T QueryString<T>(string name)
        {
            throw new NotImplementedException();
        }

        public virtual void RestartAppDomain(bool makeRedirect = false, string redirectUrl = "")
        {
            throw new NotImplementedException();
        }

        public virtual bool IsRequestBeingRedirected
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool IsPostBeingDone
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
