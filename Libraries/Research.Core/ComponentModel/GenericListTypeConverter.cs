using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Research.Core.ComponentModel
{
    /// <summary>
    /// Chịu trách nhiệm chuyển 1 chuỗi dạng 1,2,4,6 về dạng 1 list, trong đó mỗi phần tử con của list đó có kiểu T, và ngược lại
    /// 
    /// Như vậy, 1 đối tượng GenericListTypeConverter[T] có thể chuyển 1 chuỗi dạng a,b,c,d thành 1 list[T] tương ứng, và chuyển
    /// đổi ngược 1 list[t] thành chuỗi. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericListTypeConverter<T> : TypeConverter
    {
        protected readonly TypeConverter typeConverter;

        public GenericListTypeConverter()
        {
            // lấy về bộ convert dành cho kiểu T, nếu ko lấy đc thì ném ngoại lệ
            typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (typeConverter == null)
                throw new InvalidOperationException("GenericListTypeConverter: No type converter exists for type " + typeof(T).FullName);
        }
        /// <summary>
        /// Tách chuỗi input thành các chuỗi con với dấu phân cách là ","
        /// </summary>
        protected virtual IEnumerable<string> GetStringArray(string input)
        {
            if (string.IsNullOrEmpty(input)) return new string[0];
            return input.Split(',').Select(s => s.Trim());
        }

        /// <summary>
        /// Override lại để luôn chấp nhận chuyển kiêu nếu đó là string
        /// </summary>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true; // có thể chuyển kiểu nếu đó là kiểu string
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Hàm thực hiện chuyển kiểu cho đối tượng đầu vào là value. Nếu value là string thì sẽ tiến hành tách chuỗi với dâu phân cách
        /// là dấu phẩy, sau đó với mỗi chuỗi con thì sẽ chuyển nó thành kiểu T. Kết quả trả về sẽ là 1 IList[T]
        /// </summary>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                var result = new List<T>();
                foreach (var s in GetStringArray((string)value))
                {
                    object item = typeConverter.ConvertFromInvariantString(s);
                    if (item != null) result.Add((T)item);
                }
                return result;
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Hàm chuyển ngược từ 1 IList[T] thành kiểu đích destinationType. Ở đây chỉ xử lý cho kiểu đích là string,
        /// chuyển thành chuỗi phân cách bởi dấu phẩy
        /// 
        /// nếu đầu vào IList[T] == null thì sẽ trả về string.Empty
        /// </summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = string.Empty;
                if (value != null)
                {
                    IEnumerable<T> list = value as IEnumerable<T>;
                    if (list == null) return base.ConvertTo(context, culture, value, destinationType);
                    result = string.Join(",", list.Select(p => Convert.ToString(p, CultureInfo.InvariantCulture)));
                }
                return result;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
