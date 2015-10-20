using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using System.Web.SessionState;

namespace Research.Core.Fakes
{
    public class FakeHttpContext : HttpContextBase
    {
        private readonly HttpCookieCollection _cookies;
        private readonly NameValueCollection _formParams;
        private readonly NameValueCollection _queryStringParams;
        private readonly NameValueCollection _serverVariables;
        private readonly SessionStateItemCollection _sessionItems;
        private IPrincipal _principal;
        private readonly string _relativeUrl;
        private readonly string _method;
        private HttpResponseBase _response;
        private HttpRequestBase _request;
        /// <summary>
        /// Giả lập HttpContext.Items
        /// </summary>
        private readonly Dictionary<object, object> _items;

        public static FakeHttpContext Root()
        {
            return new FakeHttpContext("~/");
        }

        public FakeHttpContext(string relativeUrl, string method,
            IPrincipal principal, NameValueCollection formParams,
            NameValueCollection queryStringParams, HttpCookieCollection cookies,
            SessionStateItemCollection sessionItems, NameValueCollection serverVariables)
        {
            _relativeUrl = relativeUrl;
            _method = method;
            _principal = principal;
            _formParams = formParams;
            _queryStringParams = queryStringParams;
            _cookies = cookies;
            _sessionItems = sessionItems;
            _serverVariables = serverVariables;

            _items = new Dictionary<object, object>();
            if (_sessionItems == null) _sessionItems = new SessionStateItemCollection(); //// khác ở đây
        }

        public FakeHttpContext(string relativeUrl,
            IPrincipal principal, NameValueCollection formParams,
            NameValueCollection queryStringParams, HttpCookieCollection cookies,
            SessionStateItemCollection sessionItems, NameValueCollection serverVariables)
            : this(relativeUrl, null, principal, formParams, queryStringParams, cookies, sessionItems, serverVariables)
        {
        }

        /// <summary>
        /// Ghi chú: Những hàm tạo ko có sessionItems sẽ tạo ra đối tượng FakeHttpContext có Session ko hợp lệ ( sở hữu
        /// SessionStateItemCollection == null ,và sẽ ném ngoại lệ nếu cố truy cập )
        /// </summary>
        public FakeHttpContext(string relativeUrl, string method)
            : this(relativeUrl, method, null, null, null, null, null, null)
        {
        }

        public FakeHttpContext(string relativeUrl)
            : this(relativeUrl, null, null, null, null, null, null)
        {
        }

        public override HttpRequestBase Request
        {
            get
            {
                return _request ?? new FakeHttpRequest(_relativeUrl, _method, _formParams, _queryStringParams, _cookies, _serverVariables);
            }
        }

        public void SetRequest(HttpRequestBase request)
        {
            _request = request;
        }

        public override HttpResponseBase Response
        {
            get
            {
                return _response ?? new FakeHttpResponse();
            }
        }

        public void SetResponse(HttpResponseBase response)
        {
            _response = response;
        }

        public override IPrincipal User
        {
            get
            {
                return _principal;
            }
            set
            {
                _principal = value;
            }
        }

        /// <summary>
        ///  Mỗi lần truy cập property này lại tạo ra 1 đối tượng FakeHttpSessionState mới, dù rằng đều bao bọc quanh cùng 1 đối tượng
        ///  _sessionItems, nhưng có nên làm như thế hay ko ?
        /// </summary>
        private FakeHttpSessionState _session;
        public override HttpSessionStateBase Session
        {
            get
            {
                return _session ?? (_session = new FakeHttpSessionState(_sessionItems)); //// khác ở đây : đã sửa để chỉ dùng 1 FakeHttpSessionState duy nhất (
                // do _sessionItem là readonly nên chỉ có thể bị thay đổi trong hàm tạo, ta sẽ ko cần phải lo nó bị thay đổi ở
                // bất cứ thời điểm nào khác sau đó )
            }
        }

        public override System.Collections.IDictionary Items
        {
            get
            {
                return _items;
            }
        }

        public override bool SkipAuthorization { get; set; }

        public override object GetService(Type serviceType)
        {
            return null;
        }
    }
}
