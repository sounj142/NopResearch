
namespace Research.Core.Domain.Orders
{
    public static class GiftCardExtensions
    {
        /// <summary>
        /// Lấy về số tiền còn lại của 1 thẻ gift card, bằng cách lấy số tiền hiện hành trừ đi số tiền đã dùng trong các lịch sử sử dụng
        /// gift card. Như đã trình bày, cách này tiềm ẩn nguy cơ về đồng bộ hóa giữa bảng gift card và bảng lịch sử sử dụng. Nếu có 1 thay đổi
        /// nào đó vô ý bên bảng history sẽ khiến cho số tiền còn lại bị tính sai
        /// . Hơn nữa, cách này đòi hỏi phải lấy về danh sách các history ( 1 truy vấn, lại ko cache, cùng lắm là dựa vào cơ chế lazy
        /// loading của EF để tránh get database lần 2 ), nên sẽ chậm chạp. Sẽ hay hơn nếu ta dùng ngay 1 trường trong Gift Card để lưu
        /// giữ số dư hiện hành, như thế sẽ khỏi phải đi get history mỗi khi cần tính số tiền còn lại của thẻ
        /// </summary>
        /// <param name="giftCard"></param>
        /// <returns></returns>
        public static decimal GetGiftCardRemainingAmount(this GiftCard giftCard)
        {
            decimal result = giftCard.Amount;
            foreach (var history in giftCard.GiftCardUsageHistory)
                result -= history.UsedValue;
            if (result < decimal.Zero) result = decimal.Zero;
            return result;
        }

        /// <summary>
        /// Kiểm tra tình trạng active và số dư của thẻ, nếu lớn hơn 0 và active thì trả về true
        /// </summary>
        public static bool IsGiftCardValid(this GiftCard giftCard)
        {
            return giftCard.IsGiftCardActivated && giftCard.GetGiftCardRemainingAmount() > decimal.Zero;
        }
    }
}
