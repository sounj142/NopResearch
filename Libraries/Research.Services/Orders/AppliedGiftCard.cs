using Research.Core.Domain.Orders;

namespace Research.Services.Orders
{
    /// <summary>
    /// Applied gift card
    /// . Thẻ quả tặng đang được sử dụng để thanh toán cho yêu cầu thanh toán hiện hành ?
    /// </summary>
    public class AppliedGiftCard
    {
        /// <summary>
        /// Lượng tiền có thể sử dụng của thẻ. Con số này sẽ đc tính toán bằng cách tìm tất cả các lịch sử thanh toán đã dùng thẻ để mua, sau
        /// đó lấy số tiền của thẻ trừ đi số tiền đã dùng cho các lịch sử thanh toán này ?
        /// sẽ tốt hơn nếu trực tiếp ghi nhận thêm 1 con số thứ 2 ngay trong thẻ cho biết số tiền còn lại ???
        /// </summary>
        public decimal AmountCanBeUsed { get; set; }

        /// <summary>
        /// Thẻ GiftCard được sử dụng
        /// </summary>
        public GiftCard GiftCard { get; set; }
    }
}
