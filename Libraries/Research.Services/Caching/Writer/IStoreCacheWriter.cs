using Research.Core.Domain.Stores;
using Research.Services.Caching.Models;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface IStoreCacheWriter
    {
        /// <summary>
        /// Lấy về tất cả các Store đang có ( Cache Static )
        /// </summary>
        IList<StoreForCache> GetAll(Func<IList<StoreForCache>> acquire);

        /// <summary>
        /// Lấy về store theo Id ( đọc từ danh sách store trong static cache và cache vào perrequest cache )
        /// Đối tượng Store trả về không phải là đối tượng được lấy ra từ EF, cần trọng khi dùng
        /// </summary>
        Store GetById(int storeId, Func<Store> acquire);
    }
}
