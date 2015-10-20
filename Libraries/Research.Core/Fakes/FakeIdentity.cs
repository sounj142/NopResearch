using System;
using System.Security.Principal;

namespace Research.Core.Fakes
{
    /// <summary>
    /// Giả lập IIdentity, mô tả tên đăng nhập của người dùng trong hệ thống authenticate mặc định
    /// </summary>
    public class FakeIdentity : IIdentity
    {
        private readonly string _name;

        public FakeIdentity(string userName)
        {
            _name = userName;
        }

        public string AuthenticationType
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsAuthenticated
        {
            get { return !string.IsNullOrEmpty(_name); }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
