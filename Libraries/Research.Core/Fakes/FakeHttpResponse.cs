using System.Text;
using System.Web;

namespace Research.Core.Fakes
{
    /// <summary>
    /// Override để cài đặt đè 1 số phương thức trong HttpRequestBase theo ý muốn, giúp giả lập HttpRequestBase
    /// </summary>
    public class FakeHttpResponse: HttpResponseBase
    {
        /// <summary>
        /// Giả lập tập cookies, cho phép unit test có thể truy cập tập cookies giả lập và đánh giá lúc thực thi
        /// </summary>
        private readonly HttpCookieCollection _cookies;

        /// <summary>
        /// Giả lập dòng response
        /// </summary>
        private readonly StringBuilder _outputString;

        public FakeHttpResponse()
        {
            _cookies = new HttpCookieCollection();
            _outputString = new StringBuilder();
        }

        /// <summary>
        /// Properrty được cài đặt để dùng cho unit test. Hàm unit test sẽ gọi đến property này để lấy về nội dung hiện hành của Response
        /// qua đó đánh giá ứng dụng chạy đúng hay ko
        /// </summary>
        public string ResponseOutput
        {
            get { return _outputString.ToString(); }
        }

        /// <summary>
        /// Chặn lại để StatusCode trở thành 1 biến cục bộ của FakeReponse, dùng cho unit test
        /// </summary>
        public override int StatusCode { get; set; }

        public override string RedirectLocation { get; set; }

        public override void Write(string s)
        {
            _outputString.Append(s);
        }

        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }

        /// <summary>
        /// Đã override để chuyển hướng sang dùng tập cookies giả lập _cookies
        /// </summary>
        public override HttpCookieCollection Cookies
        {
            get
            {
                return _cookies;
            }
        }
    }
}
