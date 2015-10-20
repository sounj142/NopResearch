using Research.Core.Domain.Configuration;
using System;

namespace Research.Services.Caching.Models
{
    /// <summary>
    /// Đối tượng sẽ được dùng để lưu cache cho Setting, hơi kỳ quái ví số Properties cũng như Setting lại mất công chuyển đổi
    /// Chả nhẽ vấn đề là ở cái Serializable này và do Setting đọc từ dbcontext là sản phẩm của EF, ko được nhẹ như Setting thuần túy ?
    /// </summary>
    [Serializable]
    public partial class SettingForCache
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int StoreId { get; set; }

        public static Setting Transform(SettingForCache obj)
        {
            if (obj == null) return null;
            return new Setting
            {
                Id = obj.Id,
                Name = obj.Name,
                StoreId = obj.StoreId,
                Value = obj.Value
            };
        }

        public static SettingForCache Transform(Setting obj)
        {
            if (obj == null) return null;
            return new SettingForCache
            {
                Id = obj.Id,
                Name = obj.Name,
                StoreId = obj.StoreId,
                Value = obj.Value
            };
        }
    }
}
