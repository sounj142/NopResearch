using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core;
using Research.Core.Domain.Catalog;
using Research.Core.Domain.Stores;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Caching.Writer;
using Research.Services.Events;

namespace Research.Services.Stores
{
    public partial class StoreMappingService : BaseService<StoreMapping>, IStoreMappingService
    {
        #region Field, Property, Ctor

        /// <summary>
        /// Đối tượng ngữ cảnh dùng để lấy store hiện hành
        /// </summary>
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingCacheWriter _cacheWriter;
        private readonly CatalogSettings _catalogSettings;

        public StoreMappingService(IRepository<StoreMapping> repository,
            IStoreContext storeContext,
            IStoreMappingCacheWriter cacheWriter,
            IEventPublisher eventPublisher,
            CatalogSettings catalogSettings) :
            base(repository, eventPublisher)
        {
            _storeContext = storeContext;
            _cacheWriter = cacheWriter;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Method

        public IList<StoreMapping> GetStoreMappings<T>(T entity) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null) throw new ArgumentNullException("entity");
            int entityId = entity.Id;
            string entityName = typeof(T).Name;
            return _repository.Table.Where(p => p.EntityId == entityId && p.EntityName == entityName).ToList();
        }

        public void InsertStoreMapping<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (storeId <= 0) throw new ArgumentOutOfRangeException("storeId");

            // Nếu entity-storId đã được map từ trước đó ( có 1 trường trong StoreMapping) thì sao ?
            Insert(new StoreMapping
            {
                EntityId = entity.Id,
                EntityName = typeof(T).Name,
                StoreId = storeId
            });
        }

        /// <summary>
        /// Trả về danh sách các storeId có quyền truy cập và sử dụng đối tượng entity. Được cache static
        /// </summary>
        public int[] GetStoresIdsWithAccess<T>(T entity) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null) throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;
            return _cacheWriter.GetStoresIdsWithAccess(entityId, entityName, () => {
                return _repository.TableNoTracking
                    .Where(p => p.EntityId == entityId && p.EntityName == entityName)
                    .Select(p => p.StoreId)
                    .ToArray();
            });
        }

        public bool Authorize<T>(T entity) where T : BaseEntity, IStoreMappingSupported
        {
            return Authorize(entity, _storeContext.CurrentStore.Id);
        }

        public bool Authorize<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null || storeId < 0) return false;
            if (storeId == 0) return true; //return true if no store specified/found

            if (_catalogSettings.IgnoreStoreLimitations) // nếu cấu hình là bỏ qua mọi giới hạn mapping store
                return true;
            if (!entity.LimitedToStores) return true; // nếu đối tượng là ko bị giới hạn

            return GetStoresIdsWithAccess(entity).Contains(storeId);
        }

        #endregion
    }
}
