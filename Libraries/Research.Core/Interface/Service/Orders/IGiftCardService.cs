using Research.Core.Domain.Customers;
using Research.Core.Domain.Orders;
using System;
using System.Collections.Generic;

namespace Research.Core.Interface.Service.Orders
{
    /// <summary>
    /// quản lý giftcart, ko cache
    /// </summary>
    public partial interface IGiftCardService
    {
        void Delete(GiftCard entity);

        GiftCard GetById(int id);

        /// <summary>
        /// purchasedWithOrderId == -1 thì sẽ lấy tất cả những thẻ gift card có purchasedWithOrderId==null
        /// </summary>
        IPagedList<GiftCard> GetAllGiftCards(int? purchasedWithOrderId = null, DateTime? createdFromUtc = null, 
            DateTime? createdToUtc = null, bool? isGiftCardActivated = null, string giftCardCouponCode = null, 
            string recipientName = null,int pageIndex = 0, int pageSize = int.MaxValue);

        void Insert(GiftCard entity);

        void Update(GiftCard entity);

        /// <summary>
        /// Gets gift cards by 'PurchasedWithOrderItemId'.
        /// Lấy về gift card được mua với mã OrderItemId = purchasedWithOrderItemId, tức là khi ta có 1 mục chi tiết đơn hàng, ta có thể
        /// dò bằng hàm này để xem có gift card nào được mua bởi mục chi tiết đó hay ko
        /// </summary>
        /// <param name="purchasedWithOrderItemId">Purchased with order item identifier</param>
        /// <returns>Gift card entries</returns>
        IList<GiftCard> GetGiftCardsByPurchasedWithOrderItemId(int purchasedWithOrderItemId);

        /// <summary>
        /// Get active gift cards that are applied by a customer.
        /// Lấy về các gift card active mà đã đc sử dụng bởi người dùng này ( tức là lấy về danh sách gift card mà customer đã từng
        /// sử dụng ? )
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Active gift cards</returns>
        IList<GiftCard> GetActiveGiftCardsAppliedByCustomer(Customer customer);

        /// <summary>
        /// Generate new gift card code.
        /// Phát sinh chuỗi code cho gift card ( dùng Guid, cắt bớt ký tự để làm code )
        /// </summary>
        /// <returns>Result</returns>
        string GenerateGiftCardCode();
    }
}
