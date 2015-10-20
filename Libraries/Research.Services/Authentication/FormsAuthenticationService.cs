using System;
using System.Web;
using System.Web.Security;
using Research.Core.Domain.Customers;
using Research.Core.Interface.Service;
using Research.Core.Fakes;
using Research.Core;
using Research.Core.Infrastructure;

namespace Research.Services.Authentication
{
    public partial class FormsAuthenticationService : IAuthenticationService
    {
        private readonly HttpContextBase _httpContext;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly TimeSpan _expirationTimeSpan;

        /// <summary>
        /// Đối tượng FormsAuthenticationService sẽ đc duy trì singleton trong phạm vi lifetime scope ( thường là phạm vi request )
        /// . Do đó, việc dùng 1 field để cache lại customer hiện hành sẽ giúp tránh việc phải đọc lại customer, đồng thời cũng sẽ ko
        /// gây nhầm lẫn vì trong phạm vi vòng đời của đối tượng FormsAuthenticationService, _cachedCustomer sẽ luôn lưu đối tượng
        /// customer của người dùng hiện hành
        /// </summary>
        private Customer _cachedCustomer;

        public FormsAuthenticationService(HttpContextBase httpContext,
            ICustomerService customerService,
            CustomerSettings customerSettings)
        {
            _httpContext = httpContext;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _expirationTimeSpan = FormsAuthentication.Timeout;
        }

        /// <summary>
        /// Đăng nhập cho người dùng đã đăng ký, đối với tài khoản guest và system account thì sẽ xử lý ở nơi khác
        /// </summary>
        public virtual void SignIn(Customer customer, bool createPersistentCookie)
        {
            DateTime now = DateTime.Now;

            var ticket = new FormsAuthenticationTicket(
                1,
                _customerSettings.UsernamesEnabled ? customer.Username : customer.Email, // tùy cấu hình mà ghi nhận người dùng đăng nhập theo user name hay email
                now,
                now.Add(_expirationTimeSpan),
                createPersistentCookie,
                // lưu trữ thêm password dùng để phát hiện việc thay đổi password nếu có. 
                customer.Password,
                FormsAuthentication.FormsCookiePath
            );

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.HttpOnly = true;
            if (ticket.IsPersistent) cookie.Expires = ticket.Expiration;
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (FormsAuthentication.CookieDomain != null) cookie.Domain = FormsAuthentication.CookieDomain;

            _httpContext.Response.Cookies.Add(cookie);
            _cachedCustomer = customer; // sau khi đăng nhập cho customer, ghi nhận lại customer vào cache để tiện sử dụng sau này
        }

        public virtual void SignOut()
        {
            _cachedCustomer = null;
            if (!(_httpContext is FakeHttpContext))
                FormsAuthentication.SignOut(); // chỉ gọi đến signout nếu đây ko phải là fake request 
        }

        /// <summary>
        /// Nhìn chung là hàm thực hiện đọc cookies để lấy ra thông tin người dùng đã đăng nhập, sau đó dùng thông tin đó để truy vấn
        /// CSDL lấy ra thông tin người dùng, kiểm tra nếu thông tin ấy hợp lệ thì sẽ trả về, ngược lại thì sẽ ném ra ngoại lệ 
        /// AuthenticationDangerousException và trả về null
        /// </summary>
        public virtual Customer GetAuthenticatedCustomer()
        {
            if (_cachedCustomer != null) return _cachedCustomer; // nếu đã có cache thì sử dụng, ko cần đi sang những bước sau

            if (_httpContext == null || _httpContext.Request == null || !_httpContext.Request.IsAuthenticated
                || _httpContext.User == null || !(_httpContext.User.Identity is FormsIdentity)) return null;

            var formsIdentity = (FormsIdentity)_httpContext.User.Identity;
            string usernameOrEmail = formsIdentity.Name; //_httpContext.User.Identity.Name
            string password = formsIdentity.Ticket.UserData;
            if (string.IsNullOrWhiteSpace(usernameOrEmail)) return null;

            var customer = _customerSettings.UsernamesEnabled ?
                _customerService.GetCustomerByUsername(usernameOrEmail)
                : _customerService.GetCustomerByEmail(usernameOrEmail);

            // kiểm tra để bảo đảm tài khoản người dùng là loại role Registered, active, ko bị xóa, khớp mật khẩu
            if (customer != null && customer.Active && !customer.Deleted &&
                string.Equals(customer.Password, password, StringComparison.InvariantCulture) &&
                customer.IsRegistered())
                _cachedCustomer = customer;
            else
            {
                if (!(_httpContext is FakeHttpContext)) // chỉ ném ra ngoại lệ nếu request ko phải là fake
                    throw new AuthenticationDangerousException();
            }

            return _cachedCustomer;
        }
    }
}
