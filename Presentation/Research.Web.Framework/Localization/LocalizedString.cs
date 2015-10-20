using System;
using System.Web;

namespace Research.Web.Framework.Localization
{
    /// <summary>
    /// Đại diện cho 1 chuỗi resource. 
    /// Chuỗi resource sẽ được đọc ra từ ILocalizationService và sẽ được chuyển thành đối tượng trung gian LocalizedString ??
    /// 
    /// ? : nếu cú pháp gọi @T là T("PageTitle.CompareProducts").Text thì cứ để nó trả về 1 đối tượng string, hoặc 1 đối tượng
    /// ResourceDto nào đó, cần gì phải tạo ra lớp LocalizedString và cho nó thực thi IHtmlString, thậm chí là MarshalByRefObject
    /// 
    /// Việc cứ mỗi thao tác @T("???")  lại tạo ra 1 đối tượng LocalizedString chứa chuỗi string mà đáng ra nên được đưa
    /// thằng vào view gây ra 1 sự giảm thiểu về hiệu năng dáng kể
    /// </summary>
    public class LocalizedString : IHtmlString // thử bỏ kế thừa từ MarshalByRefObject
    {
        #region Field, property, and ctor

        /// <summary>
        /// Chuỗi resource value lưu ở đây ?
        /// </summary>
        private readonly string _localized;
        private readonly string _scope;
        private readonly string _textHint;
        private readonly object[] _args;

        /// <summary>
        /// Hàm tạo nhận vào tham số là chuỗi resource value
        /// </summary>
        public LocalizedString(string localized)
        {
            _localized = localized;
        }

        /// <summary>
        /// Chưa rõ ??
        /// </summary>
        public LocalizedString(string localized, string scope, string textHint, object[] args)
        {
            _localized = localized;
            _scope = scope;
            _textHint = textHint;
            _args = args;
        }

        public string Scope
        {
            get { return _scope; }
        }

        public string TextHint
        {
            get { return _textHint; }
        }

        public object[] Args
        {
            get { return _args; }
        }

        public string Text
        {
            get { return _localized; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Hàm kiểm tra nếu text là rỗng thì trả về defaultValue, ngược lại trả về 1 đối tượng LocalizedString chưa text
        /// </summary>
        public static LocalizedString TextOrDefault(string text, LocalizedString defaultValue)
        {
            return string.IsNullOrEmpty(text) ? defaultValue : new LocalizedString(text);
        }

        public override string ToString()
        {
            return _localized;
        }

        /// <summary>
        /// Trả về chuỗi resource value mà đối tượng đang chứa. @T trong view sẽ gọi đến hàm này
        /// </summary>
        /// <returns></returns>
        public string ToHtmlString()
        {
            return _localized;
        }

        /// <summary>
        /// Override lại để lấy hash code theo chuỗi _localized mà nó chứa ( dùng phép xor để đảo ngược bit )
        /// , cho phép dùng LocalizedString làm khóa cho dictionary
        /// </summary>
        public override int GetHashCode()
        {
            if (_localized != null) return 0 ^ _localized.GetHashCode();
            return 0;
        }

        /// <summary>
        /// Override lại để so sánh Equals() trên LocalizedString sẽ tiến hành so sánh theo chuỗi _localized được chứa.
        /// Phép so sánh sẽ được thực hiện theo Culture hiện hành
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType()) return false;

            return string.Equals(this._localized, ((LocalizedString)obj)._localized);
        }

        #endregion
    }
}
