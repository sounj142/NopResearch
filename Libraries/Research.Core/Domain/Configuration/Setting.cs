using Research.Core.Domain.Localization;

namespace Research.Core.Domain.Configuration
{
    /// <summary>
    /// Miêu tả 1 thông tin cấu hình của website, chẳng hạn như trang chủ hiển thị 10 sản phẩm, link facebook bên dưới footer là abc
    /// </summary>
    public partial class Setting: BaseEntity, ILocalizedEntity // cho phép đa ngôn ngữ
    {
        public Setting()
        { }

        public Setting(string name, string value, int storeId = 0)
        {
            this.Name = name;
            this.Value = value;
            this.StoreId = storeId;
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the store for which this setting is valid. 0 is set when the setting is for all stores
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Đã override để trả về Name
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}
