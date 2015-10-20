
namespace Research.Core.Domain.Common
{
    /// <summary>
    /// Bảng chứa các dữ liệu tạm, được tổ chức theo hình thức khóa [ entity id - tên loại entity - tên property - store id] -- Value.
    /// Thường dùng lưu các dữ liệu tạm thời của 1 số bảng, chẳng hạn như khi người dùng thực hiện thao tác check out giỏ hàng, tất cả
    /// các dữ liệu về họ tên, địa chỉ, hình thức ship, giá ship, loại thanh toán ... của người dùng nhập vào sẽ được lưu tạm vào đây ( 
    /// dữ liệu sẽ được kết với bảng Customer theo khóa ngoại entity id ), chứ ko cần phải lưu Session
    /// </summary>
    public partial class GenericAttribute : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the key group
        /// </summary>
        public string KeyGroup { get; set; }

        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }
    }
}
