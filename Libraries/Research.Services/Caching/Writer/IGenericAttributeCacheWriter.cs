using Research.Core.Domain.Common;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    public interface IGenericAttributeCacheWriter
    {
        /// <summary>
        /// Lấy về GenericAttribute theo entityId, keyGroup. Cache per request
        /// </summary>
        IList<GenericAttribute> GetAttributesForEntity(int entityId, string keyGroup, Func<IList<GenericAttribute>> acquire);

        /// <summary>
        /// Hàm lấy về 1 property cụ thể thông qua bộ 4 tham số. Cache per request
        /// </summary>
        TPropType GetAttributesForEntity<TPropType>(int entityId, string keyGroup, string key, int storeId, Func<TPropType> acquire);
    }
}
