

namespace Research.Core.Domain.Logging
{
    /// <summary>
    /// Loại hành động được ghi log, chẳng hạn như "Sửa 1 product", "Thêm 1 customer", "Xóa 1 settings". Mỗi mục log trong bảng ActivityLog
    /// được phân loại làm 1 kiểu log cụ thể, được qui định bởi bảng ActivityLogType này
    /// </summary>
    public partial class ActivityLogType : BaseEntity
    {
        /// <summary>
        /// Gets or sets the system keyword
        /// </summary>
        public string SystemKeyword { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the activity log type is enabled
        /// </summary>
        public bool Enabled { get; set; }


        public ActivityLogType MakeClone()
        {
            return new ActivityLogType
            {
                Enabled = this.Enabled,
                Id = this.Id,
                Name = this.Name,
                SystemKeyword = this.SystemKeyword
            };
        }

        public static ActivityLogType MakeClone(ActivityLogType another)
        {
            return another == null ? null : another.MakeClone();
        }
    }
}
