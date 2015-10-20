

namespace Research.Core.Domain.Stores
{
    /// <summary>
    /// Miêu tả 1 entity mà nó hỗ trợ cho store mapping. Tức là hệ thống sẽ có nhiều gian hàng, mỗi gian hàng có 1 domain khác nhau
    /// và ngoại trừ đa số dữ liệu dùng chung, có 1 số bảng mà ở đó cho phép map với Store, kết quả là mỗi store chỉ có thể show ra 1
    /// danh sách đối tượng khác nhau được map
    /// 
    /// Đối tượng cài đặt interface này sẽ có khả năng map với Store và sẽ được show ra 1 list khác nhau ở mỗi store
    /// VD: Store A chỉ show ra sản phẩm 1, 2, 3. Store B chỉ show 3, 4, 7
    /// </summary>
    public partial interface IStoreMappingSupported
    {
        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// Cờ bit == true cho biết đối tượng này là bị giới hạn vào 1 số Store nào đó, bằng false tức là đối tượng toàn cục, có thể
        /// đc xài bởi tất cả các Store
        /// </summary>
        bool LimitedToStores { get; set; }
    }
}
