using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Caching;
using Research.Core.Domain.Stores;
using Research.Services.Events;
using Research.Core.Interface.Service;
using Research.Core.Interface.Data;
using Research.Services.Caching.Writer;
using Research.Services.Caching.Models;
using Research.Core.Events;

namespace Research.Services.Stores
{
    public partial class StoreService : BaseService<Store>, IStoreService
    {
        #region Ctor, Field, Property

        private readonly IStoreCacheWriter _cacheWriter;

        public StoreService(IRepository<Store> repository,
            IEventPublisher eventPublisher,
            IStoreCacheWriter cacheWriter)
            : base(repository, eventPublisher)
        {
            _cacheWriter = cacheWriter;
        }

        #endregion

        #region Methods

        public override void Delete(Store entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (GetAllStores().Count < 2)
                // trong code xử lý, việc ném ra 1 ngoại lệ để thông báo 1 tình huống bất thường là rất bình thường
                throw new Exception("You cannot delete the only configured store");

            base.Delete(entity, true, false);

            // bổ sung thêm clear cache cho store mapping
            var event1 = new EntityDeleted<Store>(entity);
            var event2 = new EntityAllChange<StoreMapping>(null);
            _eventPublisher.Publish(event1, event2);
        }

        protected virtual IList<StoreForCache> GetAllStoresCached()
        {
            return _cacheWriter.GetAll(() =>
            {
                return _repository.TableNoTracking
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Id)
                    .Select(p => new StoreForCache
                    {
                        CompanyAddress = p.CompanyAddress,
                        CompanyName = p.CompanyName,
                        CompanyPhoneNumber = p.CompanyPhoneNumber,
                        CompanyVat = p.CompanyVat,
                        DisplayOrder = p.DisplayOrder,
                        Hosts = p.Hosts,
                        Id = p.Id,
                        Name = p.Name,
                        SecureUrl = p.SecureUrl,
                        SslEnabled = p.SslEnabled,
                        Url = p.Url
                    })
                    .ToList();
            });
        }

        public virtual IList<Store> GetAllStores()
        {
            var result = GetAllStoresCached();
            if (result == null) return null;
            return result.Select(p => StoreForCache.Transform(p)).ToList();
        }

        public override Store GetById(int id)
        {
            if (id <= 0) return null;
            return _cacheWriter.GetById(id, () => {
                var allStore = GetAllStoresCached();
                return StoreForCache.Transform(allStore.FirstOrDefault(p => p.Id == id));
            });
        }

        public override void Insert(Store entity)
        {
            base.Insert(entity, true, false);

            // bổ sung thêm clear cache cho store mapping
            var event1 = new EntityInserted<Store>(entity);
            var event2 = new EntityAllChange<StoreMapping>(null);
            _eventPublisher.Publish(event1, event2);
        }

        public override void Update(Store entity)
        {
            base.Update(entity, true, false);

            // bổ sung thêm clear cache cho store mapping
            var event1 = new EntityUpdated<Store>(entity);
            var event2 = new EntityAllChange<StoreMapping>(null);
            _eventPublisher.Publish(event1, event2);
        }

        #endregion
    }
}
