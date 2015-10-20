using System.Collections.Generic;
using Research.Core.Domain.Stores;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// service quản lý các Store trong database
    /// </summary>
    public partial interface IStoreService
    {
        void Delete(Store entity);

        /// <summary>
        /// Hàm lấy tất cả các Store, static cache
        /// </summary>
        IList<Store> GetAllStores();

        /// <summary>
        /// Lấy Store theo Id, lấy ra từ GetAllStores static cache, và được cache lại cached per request
        /// </summary>
        Store GetById(int storeId);

        void Insert(Store entity);

        void Update(Store entity);
    }
}
