using System;
using Research.Core.Domain.Localization;
using Research.Core.Domain.Stores;

namespace Research.Core.Domain.Directory
{
    /// <summary>
    /// Represents a currency
    /// 
    /// Cấu hình id loại tiền tệ mặc định dùng cho website/store được qui định ở trong bảng Settings ( CurrencySettings )
    /// </summary>
    public partial class Currency : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the currency code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// Cho biết tỷ giá : Số tiền của đơn vị tiền hiện hành cần thiết để đổi lấy 1 đơn vị tiền mặc định.
        /// Khi cập nhật bảng tỷ giá mới, hoặc khi chuyển đổi loại tiền mặc định thì cần cập nhật lại giá trị này ở tất cả các đối tượng
        /// Curency. VD con số này là 21250 với VND
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the display locale
        /// </summary>
        public string DisplayLocale { get; set; }

        /// <summary>
        /// Gets or sets the custom formatting
        /// </summary>
        public string CustomFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }
    }
}
