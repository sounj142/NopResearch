using System;
using System.Linq;
using Research.Core;
using Research.Core.Infrastructure;

namespace Research.Core.Interface.Service
{
    public static class GenericAttributeExtensions
    {
        /// <summary>
        /// Hàm trả về giả trị đã được ép kiểu phù hợp cho GenericAttribute với tên là key, đối tượng entity, và storeId tương ứng.
        /// Nên sử dụng hàm khi ta không có sẵn đối tượng genericAttributeService
        /// </summary>
        public static TPropType GetAttribute<TPropType>(this BaseEntity entity, string key, int storeId = 0)
        {
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            return GetAttribute<TPropType>(entity, key, genericAttributeService, storeId);
        }

        /// <summary>
        /// Hàm trả về giả trị đã được ép kiểu phù hợp cho GenericAttribute với tên là key, đối tượng entity, và storeId tương ứng.
        /// Nên sử dụng hàm khi ta có sẵn đối tượng genericAttributeService
        /// </summary>
        public static TPropType GetAttribute<TPropType>(this BaseEntity entity, string key, 
            IGenericAttributeService genericAttributeService, int storeId = 0)
        {
            return genericAttributeService.GetAttribute<TPropType>(entity, key, storeId);
        }
    }
}
