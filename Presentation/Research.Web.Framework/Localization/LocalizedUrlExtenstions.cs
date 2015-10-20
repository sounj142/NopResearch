using System;
using Research.Core.Domain.Localization;

namespace Research.Web.Framework.Localization
{
    public static class LocalizedUrlExtenstions
    {
        /// <summary>
        /// Chiều dài của phân đoạn ngôn ngữ, bị cố định lại là 2 ? .Đây là 1 hạn chế của Nop ?
        /// Tuy nhiên cách này giúp làm tăng tốc độ xử lý ? Thôi kệ nó đi
        /// </summary>
        private const int SEO_CODE_LENGTH = 2;

        /// <summary>
        /// Trả về true nếu ứng dụng đang phải chạy trên thư mục ảo ( dạng "/vietdung/" ) không phải dạng gốc ( dạng "/" )
        /// Điều này sẽ ảnh hưởng đến việc lấy phân đoạn ngôn ngữ, vì phân đoạn này giờ sẽ nằm ở vị trí thứ 2 : /vietdung/us/
        /// </summary>
        public static bool IsVirtualDirectory(this string applicationPath)
        {
            return applicationPath != "/";
        }

        /// <summary>
        /// Loại bỏ phân đoạn thư mục ảo nếu có ra khỏi url, chẳng hạn như /vietdung/us/login thì sẽ trả về /us/login
        /// 
        /// Để tăng tốc độ xử lý, Hàm cắt chuỗi rawUrl dựa trên 1 giả định rằng rawUrl là 1 đường dẫn con của ứng dụng web chạy trên applicationPath,
        /// tức là giả định rằng phần đầu của rawUrl chính là applicationPath 
        /// => Nếu truyền vào tham số không đúng giả định, hàm đương nhiên sẽ chạy sai
        /// </summary>
        public static string RemoveApplicationPathFromRawUrl(this string rawUrl, string applicationPath)
        {
            if (string.IsNullOrEmpty(applicationPath)) throw new ArgumentException("applicationPath must not empty");
            if (rawUrl.Length == applicationPath.Length) return "/";

            string result = rawUrl.Substring(applicationPath.Length);
            if (result.Length < 1 || result[0] != '/')
                result = "/" + result;
            return result;
        }

        /// <summary>
        /// Trả về chuỗi phân đoạn ngôn ngữ lấy từ url ( chỉ lấy đúng 2 ký tự, không lấy các dấu / ), với thư mục mà ứng dụng chạy là applicationPath ( dạng "/" vói thư mục gốc và 
        /// "/vietdung/" với thư mục con ), isRawPath cho biết url là dạng raw ( "/us/home" ) hay không ( "~/us/home" )
        /// 
        /// Ghi chú: hàm này dựa vào nhiều giả định với yêu cầu nhanh, nếu giả định sai thì sẽ ko chính xác
        /// </summary>
        public static string GetLanguageSeoCodeFromUrl(this string url, string applicationPath, bool isRawPath)
        {
            if(isRawPath)
            {
                // nếu ứng dụng đang chạy trên thư mục ảo thì bỏ phân đoạn thư mục ảo đi trước khi xử lý tiếp
                if (applicationPath.IsVirtualDirectory())
                    url = url.RemoveApplicationPathFromRawUrl(applicationPath);
                
                // cách xử lý này nhanh, nhưng hơi bị ẩu. Nó cố định chiều dài phân đoạn ngôn ngữ, và cứ thế lấy 2 ký tự làm phân đoạn ngôn ngữ,
                // hồn nhiên ko kiểm tra xem sau 2 ký tự đó có phải là dấu / hay là kết thúc chuỗi hay ko, cũng ko kiểm
                // tra xem phân đoạn lấy ra được có phỉ là 1 phân đoạn hợp lệ khi so với các phân doạn trong bảng Language hay ko
                return url.Substring(1, SEO_CODE_LENGTH);
            }
            // ko phải raw thì sẽ có dạng "~/..."
            return url.Substring(2, SEO_CODE_LENGTH); 
        }

        /// <summary>
        /// Trả về true nếu url có phân đoạn ngôn ngữ
        /// Việc kiểm tra chỉ đảm bảo phân đoạn ngôn ngữ đúng theo cú pháp, có SEO_CODE_LENGTH ký tự
        /// , còn việc có language nào định nghĩa phân đoạn đó hay ko thì ko cần biết
        /// </summary>
        public static bool IsLocalizedUrl(this string url, string applicationPath, bool isRawPath)
        {
            if(string.IsNullOrEmpty(url)) return false;
            int addNum = 2;
            if(isRawPath)
            {
                if (applicationPath.IsVirtualDirectory())
                    url = url.RemoveApplicationPathFromRawUrl(applicationPath);
                addNum = 1;
            }

            int length = url.Length, value = SEO_CODE_LENGTH + addNum;
            return length == value || (length > value && url[value] == '/');
        }
        
        /// <summary>
        /// Loại bỏ phân đoạn language khỏi raw Url ( raw, ko phải là dạng ~/... )
        /// VD link /vietdung/vn/may-tinh se thanh /vietdung/may-tinh
        /// </summary>
        public static string RemoveLanguageSeoCodeFromRawUrl(this string url, string applicationPath)
        {
            if(string.IsNullOrEmpty(url)) return url;

            string result = null;
            bool isVirtualDirectory = applicationPath.IsVirtualDirectory();
            if (isVirtualDirectory)
                url = url.RemoveApplicationPathFromRawUrl(applicationPath);
            int length = url.Length, value = SEO_CODE_LENGTH + 1;
            if (length < value)
                result = url;
            else if (length == value)
                result = "/";
            else result = url.Substring(value);

            if (isVirtualDirectory)
            {
                if (applicationPath[applicationPath.Length - 1] == '/')
                    result = applicationPath.Substring(0, applicationPath.Length - 1) + result;
                else
                    result = applicationPath + result;
            }  

            return result;
        }

        /// <summary>
        /// Add thêm phân đoạn ngôn ngữ language vào RAW URL
        /// </summary>
        public static string AddLanguageSeoCodeToRawUrl(this string url, string applicationPath, Language language)
        {
            if (language == null) throw new ArgumentNullException("language");

            int startIndex = applicationPath.IsVirtualDirectory() ? applicationPath.Length : 0;
            url = url.Insert(startIndex, "/" + language.UniqueSeoCode);
            return url;
        }

        /// <summary>
        /// Thay thế phân đoạn ngôn ngữ đang có của RawUrl thành phân đoạn mới cho bởi language
        /// </summary>
        /// <param name="url"></param>
        /// <param name="applicationPath"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string ReplaceLanguageSeoCodeToRawUrl(this string url, string applicationPath, Language language)
        {
            if (language == null) throw new ArgumentNullException("language");
            if (string.IsNullOrEmpty(url)) return url;

            string result = null;
            bool isVirtualDirectory = applicationPath.IsVirtualDirectory();
            if (isVirtualDirectory)
                url = url.RemoveApplicationPathFromRawUrl(applicationPath);
            int length = url.Length, value = SEO_CODE_LENGTH + 1;
            if (length < value)
                result = url;
            else if (length == value)
                result = "/";
            else result = url.Substring(value);

            if (isVirtualDirectory)
            {
                if (applicationPath[applicationPath.Length - 1] == '/')
                    result = applicationPath.Substring(0, applicationPath.Length - 1) + "/" + language.UniqueSeoCode + result;
                else
                    result = applicationPath + "/" + language.UniqueSeoCode + result;
            }

            return "/" + language.UniqueSeoCode + result;
        }
    }
}
