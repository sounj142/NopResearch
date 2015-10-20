using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Directory;
using Research.Core.Interface.Service;
using Research.Core.Interface.Data;
using Research.Services.Events;
using Research.Core.Domain.Stores;
using Research.Core;
using Research.Core.Domain.Catalog;
using Research.Services.Caching.Writer;
using Research.Core.Infrastructure;

namespace Research.Services.Directory
{
    // LƯU Ý :
    // Trường hợp thực hiện lời gọi như sau :
    //  IEnumerable<Country> result = _repository.Table.Where(p => c.Id % 2 == 0);
    //  result = result.Where(p => c.Id >= 40);
    //
    //  var kq = result.ToList();
    // Thao tác truy cập database sẽ vẫn bị lazy đến khi hàm ToList() được gọi, tuy nhiên vì result ở thời điểm gọi Where thứ 2 là
    // 1 IEnumerable nên thao tác where thứ 2: Where(p => c.Id >= 40) sẽ không được đưa vào câu truy vấn database
    // Kết quả là truy vấn database sẽ chỉ thực hiện với truy vấn của mệnh đề Where số 1, Where(p => c.Id % 2 == 0); trả về kết quả là
    // TẤT CẢ COUNTRY CÓ ID CHẴN, sau đó mệnh đề where 2 mới được áp dụng ( lazy ) trên tập kết quả này trong bộ nhớ

    // => ĐỂ CÓ THỂ MANG TRUY VẤN WHERE VÀO DATABASE, CẦN ĐẢM BẢO ĐÓ LÀ IQueryable<> để có thể triệu gọi phiên bản Where() 
    // được định nghĩa riêng cho IQueryable




    public partial class CountryService : BaseService<Country>, ICountryService
    {
        #region field, ctor, property

        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICountryCacheWriter _cacheWriter;
        
        private IStoreMappingService _storeMappingService;

        public CountryService(IRepository<Country> repository,
            IEventPublisher eventPublisher,
            IRepository<StoreMapping> storeMappingRepository,
            IStoreContext storeContext,
            CatalogSettings catalogSettings,
            ICountryCacheWriter cacheWriter)
            : base(repository, eventPublisher)
        {
            _storeMappingRepository = storeMappingRepository;
            _storeContext = storeContext;
            _catalogSettings = catalogSettings;
            _cacheWriter = cacheWriter;
        }

        #endregion

        #region ultilities

        /// <summary>
        /// lấy tất cả country, static cache, sắp xếp theo DisplayOrder - Name
        /// </summary>
        /// <returns></returns>
        protected virtual IList<Country> GetAllFromStaticCache()
        {
            return _cacheWriter.GetAllFromStaticCache(() =>
            {
                return _repository.TableNoTracking
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToList();
            });
        }

        #endregion

        #region methods

        // cache per request
        public virtual IList<Country> GetAllCountries(bool autoOrder = true, bool showHidden = false, bool getFromStaticCache = true)
        {
            return _cacheWriter.GetAllCountries(autoOrder, showHidden, getFromStaticCache, () => {
                if (getFromStaticCache)
                {
                    IEnumerable<Country> allCountries = GetAllFromStaticCache();
                    if (!showHidden)
                    {
                        allCountries = allCountries.Where(c => c.Published);
                        // storeMapping
                        if (!_catalogSettings.IgnoreStoreLimitations)
                        {
                            // nếu như cờ showhidden đc tắt ( giao diện người dùng ), và cấu hình cho phép giới hạn store mapping
                            int currentStoreId = _storeContext.CurrentStore.Id;
                            if (_storeMappingService == null)
                                _storeMappingService = EngineContext.Current.Resolve<IStoreMappingService>();
                            allCountries = allCountries.Where(c => !c.LimitedToStores ||
                                _storeMappingService.Authorize(c, currentStoreId));
                        }
                    }
                    // dữ liệu trong static cache đã đc sắp sãn nên autoOrder hay ko đều ko ảnh hưởng
                    return allCountries.ToList();
                }
                else
                {
                    var query = _repository.Table;
                    if (!showHidden)
                    {
                        query = query.Where(c => c.Published);
                        if (!_catalogSettings.IgnoreStoreLimitations)
                        {
                            // nếu như cờ showhidden đc tắt ( giao diện người dùng ), và cấu hình cho phép giới hạn store mapping
                            int currentStoreId = _storeContext.CurrentStore.Id;
                            // cách của mình
                            query = query.Where(c => !c.LimitedToStores ||
                                _storeMappingRepository.Table.Any(m => m.EntityId == c.Id &&
                                    m.EntityName == "Country" && m.StoreId == currentStoreId));

                            // cách của Nop
                            //query = from c in query
                            //        join sc in _storeMappingRepository.Table
                            //        on new { c1 = c.Id, c2 = "Country" } equals new { c1 = sc.EntityId, c2 = sc.EntityName } into c_sc
                            //        from sc in c_sc.DefaultIfEmpty()
                            //        where !c.LimitedToStores || sc.StoreId == currentStoreId
                            //        select c;
                            //query = from c in query
                            //        group c by c.Id
                            //            into cGroup
                            //            orderby cGroup.Key
                            //            select cGroup.FirstOrDefault();
                        }
                    }

                    if (autoOrder) query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);
                    return query.ToList();
                }
            });
        }

        public virtual IList<Country> GetAllCountriesForBilling(bool autoOrder = true, bool showHidden = false, bool getFromStaticCache = true)
        {
            return GetAllCountries(autoOrder, showHidden, getFromStaticCache)
                .Where(c => c.AllowsBilling).ToList();
        }

        public virtual IList<Country> GetAllCountriesForShipping(bool autoOrder = true, bool showHidden = false, bool getFromStaticCache = true)
        {
            return GetAllCountries(autoOrder, showHidden, getFromStaticCache)
                .Where(c => c.AllowsShipping).ToList();
        }

        public virtual Country GetCountryById(int countryId, bool getFromStaticCache = true)
        {
            if (countryId <= 0) return null;

            return _cacheWriter.GetCountryById(countryId, getFromStaticCache, () =>
            {
                if (getFromStaticCache)
                    return Country.MakeClone(GetAllFromStaticCache().FirstOrDefault(c => c.Id == countryId));
                else return _repository.GetById(countryId);
            });
        }

        public virtual IList<Country> GetCountriesByIds(int[] countryIds, bool getFromStaticCache = true)
        {
            if (countryIds == null || countryIds.Length == 0) return new List<Country>();

            IList<Country> data;
            if(getFromStaticCache)
            {
                data = GetAllFromStaticCache().Where(c => countryIds.Contains(c.Id))
                    .Select(c => c.MakeClone()).ToList();
            }else
            {
                data = _repository.Table
                    .Where(c => countryIds.Contains(c.Id)).ToList();
            }
            var result = new List<Country>();
            foreach(int id in countryIds)
                foreach(var country in data)
                    if (id == country.Id)
                    {
                        result.Add(country);
                        break;
                    }
            return result;
        }

        public virtual Country GetCountryByTwoLetterIsoCode(string twoLetterIsoCode, bool getFromStaticCache = true)
        {
            if (string.IsNullOrEmpty(twoLetterIsoCode)) return null;

            if (getFromStaticCache)
            {
                return Country.MakeClone(GetAllFromStaticCache().FirstOrDefault(c =>
                    twoLetterIsoCode.Equals(c.TwoLetterIsoCode, StringComparison.InvariantCultureIgnoreCase)));
            }
            else
            {
                return _repository.Table.FirstOrDefault(c => c.TwoLetterIsoCode == twoLetterIsoCode);
            }
        }

        public virtual Country GetCountryByThreeLetterIsoCode(string threeLetterIsoCode, bool getFromStaticCache = true)
        {
            if (string.IsNullOrEmpty(threeLetterIsoCode)) return null;

            if (getFromStaticCache)
            {
                return Country.MakeClone(GetAllFromStaticCache().FirstOrDefault(c =>
                    threeLetterIsoCode.Equals(c.ThreeLetterIsoCode, StringComparison.InvariantCultureIgnoreCase)));
            }
            else
            {
                return _repository.Table.FirstOrDefault(c => c.ThreeLetterIsoCode == threeLetterIsoCode);
            }
        }

        #endregion
    }
}
