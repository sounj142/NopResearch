
namespace Research.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a gift card type. Có 2 loại thẻ: thẻ vật lý ( in ra phát cho khách hàng ) và thẻ ảo ( chỉ có 1 con số thẻ )
    /// </summary>
    public enum GiftCardType
    {
        /// <summary>
        /// Virtual
        /// </summary>
        Virtual = 0,
        /// <summary>
        /// Physical
        /// </summary>
        Physical = 1,
    }
}
