using Research.Core.Domain.Orders;
using Research.Core;
using Research.Core.Domain.Customers;
using Research.Core.Interface.Service.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Interface.Data;
using Research.Services.Events;

namespace Research.Services.Orders
{
    public partial class GiftCardService : BaseService<GiftCard>, IGiftCardService
    {
        #region fields, ctors, properties

        public const int GiftCardCodeLength = 13;

        public GiftCardService(IRepository<GiftCard> repository, IEventPublisher eventPublisher)
            : base(repository, eventPublisher)
        { }

        #endregion

        #region methods

        public virtual IPagedList<GiftCard> GetAllGiftCards(int? purchasedWithOrderId = null, DateTime? createdFromUtc = null, 
            DateTime? createdToUtc = null, bool? isGiftCardActivated = null, string giftCardCouponCode = null, 
            string recipientName = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _repository.Table;
            if (purchasedWithOrderId.HasValue)
                if (purchasedWithOrderId.Value == -1)
                    query = query.Where(p => p.PurchasedWithOrderItemId == null);
                else
                    query = query.Where(p => p.PurchasedWithOrderItemId == purchasedWithOrderId);
            if (createdFromUtc.HasValue)
                query = query.Where(p => p.CreatedOnUtc >= createdFromUtc.Value);
            if (createdToUtc.HasValue)
                query = query.Where(p => p.CreatedOnUtc <= createdToUtc.Value);
            if (isGiftCardActivated.HasValue)
                query = query.Where(p => p.IsGiftCardActivated == isGiftCardActivated.Value);

            if (!string.IsNullOrEmpty(giftCardCouponCode))
                query = query.Where(p => p.GiftCardCouponCode == giftCardCouponCode);
            if (!string.IsNullOrWhiteSpace(recipientName))
                query = query.Where(p => p.RecipientName.Contains(recipientName));
            query = query.OrderByDescending(p => p.CreatedOnUtc);

            return new PagedList<GiftCard>(query, pageIndex, pageSize);
        }

        public virtual IList<GiftCard> GetGiftCardsByPurchasedWithOrderItemId(int purchasedWithOrderItemId)
        {
            if (purchasedWithOrderItemId <= 0) return new List<GiftCard>();

            return _repository.Table.Where(p => p.PurchasedWithOrderItemId == purchasedWithOrderItemId)
                .OrderBy(p => p.Id)
                .ToList();
        }

        public virtual IList<GiftCard> GetActiveGiftCardsAppliedByCustomer(Customer customer)
        {
            var result = new List<GiftCard>();
            if (customer == null) return result;

            var couponCodes = customer.ParseAppliedGiftCardCouponCodes();
            if (couponCodes.Count > 0)
            {
                var giftCards = _repository.Table.Where(p => p.IsGiftCardActivated && couponCodes.Contains(p.GiftCardCouponCode)).ToList();
                foreach (var card in giftCards)
                    if (card.IsGiftCardValid()) result.Add(card);
            }

            return result;
        }

        public virtual string GenerateGiftCardCode()
        {
            string result = Guid.NewGuid().ToString();
            if (result.Length > GiftCardCodeLength) result = result.Substring(0, GiftCardCodeLength);
            return result;
        }

        #endregion
    }
}
