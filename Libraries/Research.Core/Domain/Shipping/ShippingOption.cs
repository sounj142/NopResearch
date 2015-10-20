using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Research.Core.Domain.Shipping
{
    /// <summary>
    /// Miêu tả 1 tùy chọn shipping, thường dùng cho việc biểu diễn 1 coupon code. 1 đối tượng Customer thường tự định nghĩa thêm 1 vài
    /// generic attribute thuộc kiểu ShippingOption. Việc chuyển đổi ShippingOption để lưu trữ như 1 chuỗi vào GenericAttribute sẽ do
    /// lớp TypeConvert bên dưới thực hiện
    /// </summary>
    public partial class ShippingOption
    {
        /// <summary>
        /// Gets or sets the system name of shipping rate computation method
        /// </summary>
        public string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets a shipping rate (without discounts, additional shipping charges, etc)
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets a shipping option name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a shipping option description
        /// </summary>
        public string Description { get; set; }
    }

    public class ShippingOptionTypeConverter : TypeConverter
    {
        private static readonly XmlSerializer _xmlSerializer = new XmlSerializer(typeof(ShippingOption));

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string valueStr = value as string;
            if (valueStr != null)
            {
                ShippingOption shippingOption = null;
                if (!string.IsNullOrEmpty(valueStr))
                {
                    // khác: bỏ try catch ở chỗ này để xem có vấn đề gì ko
                    using(var reader = new StringReader(valueStr))
                    {
                        shippingOption = (ShippingOption)_xmlSerializer.Deserialize(reader);
                    }
                }
                return shippingOption;
            }
            
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var shippingOption = value as ShippingOption;
                if(shippingOption != null)
                {
                    var builder = new StringBuilder();
                    using (var writer = new StringWriter(builder))
                    {
                        _xmlSerializer.Serialize(writer, value);
                        return builder.ToString();
                    }
                }
                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class ShippingOptionListTypeConverter : TypeConverter
    {
        private static readonly XmlSerializer _xmlSerializer = new XmlSerializer(typeof(List<ShippingOption>));

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string valueStr = value as string;
            if (valueStr != null)
            {
                List<ShippingOption> shippingOptions = null;
                if (!string.IsNullOrEmpty(valueStr))
                {
                    // khác: bỏ try catch ở chỗ này để xem có vấn đề gì ko
                    using (var reader = new StringReader(valueStr))
                    {
                        shippingOptions = (List<ShippingOption>)_xmlSerializer.Deserialize(reader);
                    }
                }
                return shippingOptions;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var shippingOptions = value as List<ShippingOption>;
                if (shippingOptions != null)
                {
                    var builder = new StringBuilder();
                    using (var writer = new StringWriter(builder))
                    {
                        _xmlSerializer.Serialize(writer, value);
                        return builder.ToString();
                    }
                }
                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
