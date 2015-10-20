using System;

namespace Research.Core
{
    /// <summary>
    /// Ngoại lệ được ném ra khi quá trình kiếm tra cookies authentication do máy client gửi tới phát hiện có vấn đề chẳng hạn như: Ko tìm thấy
    /// tài khoản tương ứng với username/email, password ko khớp, tài khoản bị ban, bị xóa ....v....v....
    /// </summary>
    public class AuthenticationDangerousException : Exception
    {
        public AuthenticationDangerousException() 
        { 
        }

        public AuthenticationDangerousException(string message)
            : base(message)
        {
        }
    }
}
