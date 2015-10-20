using System;
using System.Linq;
using Research.Core;
using Research.Core.Domain;
//using Research.Services.Common;

namespace Research.Web.Framework.Themes
{
    /// <summary>
    /// Đối tương ngữ cảnh theme, cho phép lấy ra theme hiện hành ( theme này sẽ là khác nhau tùy người dùng / Store )
    /// </summary>
    public partial class ThemeContext : IThemeContext //// Khác, ko có cài đặt cho using Research.Core.Domain.Customers
    {

        public string WorkingThemeName
        {
            get
            {
                return "DefaultClean"; // tạm thời cài đặt tĩnh
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
