using Research.Core.ComponentModel;
using Research.Core.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace Research.Core
{
    public partial class CommonHelper
    {
        public const int EnsureSubscriberEmailMaxLength = 255;

        /// <summary>
        /// hàm kiểm tra 1 email có hợp lệ hay ko, nếu ko ném ngoại lệ. Tự động cắt chuỗi nếu chuỗi đưa vào > 255 ký tự
        /// </summary>
        public static string EnsureSubscriberEmailOrThrow(string email)
        {
            if (email != null) email = email.Trim();
            email = EnsureMaximumLength(email, EnsureSubscriberEmailMaxLength);
            if (!IsValidEmail(email)) throw new ResearchException("Email is not valid.");
            return email;
        }

        /// <summary>
        /// Ensure that a string is not null
        /// </summary>
        public static string EnsureNotNull(string str)
        {
            return str == null ? string.Empty : str;
        }

        /// <summary>
        /// Ensure that a string doesn't exceed maximum allowed length
        /// </summary>
        /// <param name="str">Input string</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="postfix">A string to add to the end if the original string was shorten</param>
        /// <returns>Input string if its lengh is OK; otherwise, truncated input string</returns>
        public static string EnsureMaximumLength(string str, int maxLength, string postfix = null)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (str.Length > maxLength)
            {
                str = str.Substring(0, maxLength);
                if (!string.IsNullOrEmpty(postfix))
                    str += postfix;
            }
            return str;
        }

        /// <summary>
        /// Verifies that a string is in valid e-mail format
        /// </summary>
        /// <param name="email">Email to verify</param>
        /// <returns>true if the string is a valid e-mail address and false if it's not</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email.Trim(), "^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Generate random digit code
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Result string</returns>
        public static string GenerateRandomDigitCode(int length, Random random = null)
        {
            if(random == null) random = new Random();
            string str = string.Empty;
            for (int i = 0; i < length; i++)
                str = String.Concat(str, random.Next(10).ToString());
            return str;
        }

        /// <summary>
        /// Convert giá trị value về kiểu đích destinationType
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <param name="culture">Culture</param>
        /// <returns>The converted value.</returns>
        public static object To(object value, Type destinationType, CultureInfo culture)
        {
            if (value == null) return null;

            var sourceType = value.GetType();
            var destinationConverter = GetNopCustomTypeConverter(destinationType);
            if (destinationConverter != null && destinationConverter.CanConvertFrom(sourceType))
                return destinationConverter.ConvertFrom(null, culture, value);

            var sourceConverter = GetNopCustomTypeConverter(sourceType);
            if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
                return sourceConverter.ConvertTo(null, culture, value, destinationType);

            if (destinationType.IsEnum && (value is int || value is long))
                return Enum.ToObject(destinationType, value);
            if (!destinationType.IsInstanceOfType(value))
                return Convert.ChangeType(value, destinationType, culture);
            return null;
        }

        public static object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Cho phép convert 1 giá trị value về kiểu T. 1 số kiểu List[X] sẽ sử dụng GenericListTypeConverter để chuyển đổi từ dạng
        /// 1,2,3,4 thanh List và ngược lại. Các kiểu khác dùng bộ converter mặc định
        /// </summary>
        public static T To<T>(object value)
        {
            return (T)To(value, typeof(T));
        }

        /// <summary>
        /// Lấy về bộ chuyển đổi kiểu phù hợp với kiểu type ( ghi nhớ : Bộ chuyển đổi kiểu là 2 chiều, cho phép chuyển từ kiểu X nào đó
        /// thành type, và chuyển ngược type thành kiểu X nào đó )
        /// 
        /// Với 1 số loại List, sẽ trả về 1 đối tượng GenericListTypeConverter, ngược lại sẽ phó thác cho 
        /// TypeDescriptor.GetConverter()
        /// </summary>
        public static TypeConverter GetNopCustomTypeConverter(Type type)
        {
            //we can't use the following code in order to register our custom type descriptors
            //TypeDescriptor.AddAttributes(typeof(List<int>), new TypeConverterAttribute(typeof(GenericListTypeConverter<int>)));
            //so we do it manually here

            if (type == typeof(List<int>)) return new GenericListTypeConverter<int>();
            if (type == typeof(List<decimal>)) return new GenericListTypeConverter<decimal>();
            if (type == typeof(List<string>)) return new GenericListTypeConverter<string>();
            if (type == typeof(ShippingOption)) return new ShippingOptionTypeConverter();
            if (type == typeof(List<ShippingOption>)) return new ShippingOptionListTypeConverter(); // khác chỗ này

            return TypeDescriptor.GetConverter(type);
        }

        /// <summary>
        /// Gía trị cache static dùng để lưu mức trust level của app, cho phép việc lấy ra giá trị trust level chỉ cần thực hiện 1 lần
        /// trong suốt vòng đời ứng dụng
        /// </summary>
        private static AspNetHostingPermissionLevel? _trustLevel;

        /// <summary>
        /// Trả về mức độ trust level của app. Giá trị lấy được sẽ đc cache vào 1 biến static, giúp cho các yêu cầu lấy trust level về
        /// sau chỉ ở O(1)
        /// </summary>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if(!_trustLevel.HasValue)
            {
                _trustLevel = AspNetHostingPermissionLevel.None;
                // lần lượt kiểm tra mức trust level từ cao xuống thấp, dừng lại ở mức cao nhất đạt được
                foreach(var trustLevel in new [] { 
                    AspNetHostingPermissionLevel.Unrestricted,
                    AspNetHostingPermissionLevel.High,
                    AspNetHostingPermissionLevel.Medium,
                    AspNetHostingPermissionLevel.Low,
                    AspNetHostingPermissionLevel.Minimal 
                })
                {
                    try
                    {
                        new AspNetHostingPermission(trustLevel).Demand();
                        _trustLevel = trustLevel;
                        break; //we've set the highest permission we can
                    }catch(SecurityException)
                    {
                        continue;
                    }
                }
            }
            return _trustLevel.Value;
        }
    }
}
