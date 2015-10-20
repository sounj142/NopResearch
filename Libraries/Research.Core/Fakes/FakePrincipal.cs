using System;
using System.Linq;
using System.Security.Principal;

namespace Research.Core.Fakes
{
    /// <summary>
    /// Giả lập đối tượng IPrincipal dùng để nhận diện tên người dùng đăng nhập và quyền của người đó trong hệ thống authenticate mặc
    /// định của microsoft
    /// </summary>
    public class FakePrincipal: IPrincipal
    {
        /// <summary>
        /// Đối tượng nhận diện tên người dùng
        /// </summary>
        private readonly IIdentity _identity;
        /// <summary>
        /// Danh sách quyền của người dùng
        /// </summary>
        private readonly string[] _roles;

        public FakePrincipal(IIdentity identity, string[] roles)
        {
            this._identity = identity;
            this._roles = roles;
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }

        public bool IsInRole(string role)
        {
            return _roles != null && _roles.Contains(role, StringComparer.InvariantCulture);
        }
    }
}
