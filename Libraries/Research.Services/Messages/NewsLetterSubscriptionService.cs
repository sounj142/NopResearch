using System;
using System.Linq;
using Research.Core.Domain.Messages;
using Research.Core.Interface.Service;

namespace Research.Services.Messages
{
    public partial class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        public void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
        }

        public NewsLetterSubscription GetNewsLetterSubscriptionByEmailAndStoreId(string email, int storeId)
        {
            return null;
        }
    }
}
