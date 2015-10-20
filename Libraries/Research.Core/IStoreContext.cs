using Research.Core.Domain.Stores;

namespace Research.Core
{
    /// <summary>
    /// Đối tượng ngữ cảnh store hiện hành. Store hiện hành sẽ được lấy từ domain hiện hành
    /// </summary>
    public interface IStoreContext
    {
        /// <summary>
        /// Lấy về store hiện hành ( được lấy thông qua domain hiện hành )
        /// </summary>
        Store CurrentStore { get; }
    }
}
