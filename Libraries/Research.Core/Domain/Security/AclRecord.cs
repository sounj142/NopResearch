using Research.Core.Domain.Customers;

namespace Research.Core.Domain.Security
{
    // có 1 mục cấu hình settings cho phép bỏ qua bảng AclRecord ánh xạ quyền các Role - entity, cho bởi CatalogSettings.IgnoreAcl



    /// <summary>
    /// Represents an ACL record
    /// Cấu trúc bảng tương tự như StoreMapping. Nếu như StoreMapping là dùng để ánh xạ giữa những đối tượng entity khác và Store, qui định
    /// những đối tượng nào bị giới hạn trong phạm vi của những store nào thì AclRecord là để giới hạn những entity( cài đặt IAclSupported )
    /// với bảng CustomerRole. Như vậy ta sẽ có 1 ánh xạ giữa các entity cài đặt IAclSupported và CustomerRole, đồng thời mỗi entity
    /// thuộc loại IAclSupported đều có 1 trường bool SubjectToAcl dùng để nhận diện xem nó có phải loại giới hạn CustomerRole hay ko 
    /// 
    /// 
    /// 
    /// === Nói chung là rất giống StoreMapping, nhưng dùng để map giữa entity và CustomerRole, 
    /// Vai trò cụ thể là để tạo nên những ngoại lệ về phân quyền. Bình thường thì phân quyền qui định theo PermissionRecord, kiểu như
    /// với quyền con ManagerInventory sẽ có quyền quản lý inventory, và 1 CustomerRole muốn quản lý inventory sẽ phải được ánh xạ với
    /// quyền con ManagerInventory đó. Yêu cầu đặt ra là khi ta muốn 1 CustomerRole nào đó có thể thao tác trên 1 số lượng đối tượng hạn chế
    /// mà ko phải áp quyền con tương ứng lên Role đó, khi đó ta dùng AclRecord để map trực tiếp các đối tượng đến CustomerRole, tạo ra
    /// ngoại lệ là Role có thể tương tác với 1 số các đối tượng do AclRecord qui định mà ko cần phải có quyền tương ứng trong PermissionRecord
    /// </summary>
    public partial class AclRecord : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier
        /// </summary>
        public int CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the customer role
        /// </summary>
        public virtual CustomerRole CustomerRole { get; set; }
    }
}
