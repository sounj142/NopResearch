using Research.Core.Domain.Messages;
using System;

namespace Research.Core.Interface.Service
{
    public partial interface INewsLetterSubscriptionService
    {

        NewsLetterSubscription GetNewsLetterSubscriptionByEmailAndStoreId(string email, int storeId);

        void UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        
    }
}
