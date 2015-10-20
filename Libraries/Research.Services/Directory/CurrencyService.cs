using Research.Core;
using Research.Core.Domain.Directory;
using Research.Core.Domain.Localization;
using Research.Core.Domain.Stores;
using Research.Core.Events;
using Research.Core.Infrastructure;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Core.Plugins;
using Research.Services.Caching.Models;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Services.Directory
{
    public partial class CurrencyService : BaseService<Currency>, ICurrencyService
    {
        #region Fields, Properties, Ctors

        private readonly IStoreMappingService _storeMappingService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly ICurrencyCacheWriter _cacheWriter;


        public CurrencyService(IRepository<Currency> repository,
            IStoreMappingService storeMappingService,
            CurrencySettings currencySettings,
            IPluginFinder pluginFinder,
            ICurrencyCacheWriter cacheWriter,
            IEventPublisher eventPublisher)
            : base(repository, eventPublisher)
        {
            _storeMappingService = storeMappingService;
            _currencySettings = currencySettings;
            _pluginFinder = pluginFinder;
            _cacheWriter = cacheWriter;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Chịu trách nhiệm lấy về danh sách các Currency từ static cache. Để đảm bảo tính chỉ đọc của cache tránh ghi nhầm, chúng
        /// ta sẽ cache toàn bộ vào đối tượng trung gian CurrencyForCache
        /// </summary>
        protected IList<CurrencyForCache> GetAllCurrenciesCached()
        {
            return _cacheWriter.GetAll(() =>
            {
                return _repository.TableNoTracking
                    .OrderBy(p => p.DisplayOrder) // sắp xếp sẵn theo DisplayOrder
                    .Select(obj => new CurrencyForCache
                    {
                        CreatedOnUtc = obj.CreatedOnUtc,
                        CurrencyCode = obj.CurrencyCode,
                        CustomFormatting = obj.CustomFormatting,
                        DisplayLocale = obj.DisplayLocale,
                        DisplayOrder = obj.DisplayOrder,
                        Id = obj.Id,
                        LimitedToStores = obj.LimitedToStores,
                        Name = obj.Name,
                        Published = obj.Published,
                        Rate = obj.Rate,
                        UpdatedOnUtc = obj.UpdatedOnUtc
                    }).ToList();
            });
        }

        #endregion

        #region Methods

        public IList<ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            var exchangeRateProvider = LoadActiveExchangeRateProvider();
            if (exchangeRateProvider == null)
                throw new Exception("Hệ thống ko có IExchangeRateProvider dùng để cập nhật tỷ giá, vui lòng cung cấp interface này ( plugin )");
            return exchangeRateProvider.GetCurrencyLiveRates(exchangeRateCurrencyCode);
        }

        /// <summary>
        /// Khác với cài đặt trong NOP khi mà NOP chỉ xóa dữ liệu tại bảng Currency và để lại 1 mớ rác tại các bảng LocalizedProperty,
        /// StoreMapping, ( và có thể còn các bảng khác nữa ). Cài đặt Delete ở đây sẽ nỗ lực xóa hết dữ liệu liên quan tại các bảng
        /// này, cài đặt các thao tác xóa trong phạm vi của 1 transaction
        /// </summary>
        public override void Delete(Currency entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (entity.Id <= 0) throw new ArgumentException("Currency to delete must have valid id");

            var engine = EngineContext.Current;
            var localizedEntityService = engine.Resolve<ILocalizedEntityService>();

            var listMapping = _storeMappingService.GetStoreMappings(entity);
            var listLocalizedProperty = localizedEntityService.GetLocalizedProperties(entity);


            using (var transaction = _unitOfWork.BeginTransaction())
            {
                if (listMapping.Count > 0) // có ánh xạ đến bảng store, tiến hành xóa
                {
                    var storeMappingRepository = engine.Resolve<IRepository<StoreMapping>>();
                    storeMappingRepository.Delete(listMapping);
                }
                if (listLocalizedProperty.Count > 0)
                {
                    var localizedPropertyRepository = engine.Resolve<IRepository<LocalizedProperty>>();
                    localizedPropertyRepository.Delete(listLocalizedProperty);
                }

                _repository.Delete(entity);
                _unitOfWork.SaveChanges();
                transaction.Commit();
            }
            
            // phát ra thông báo clear cache tổng hợp cho 3 loại đối tượng nếu cần
            var currencyEvent = new EntityDeleted<Currency>(entity);
            EntityAllChange<StoreMapping> storeMappingEvent = null; // nếu bằng null thì sự kiện tương ứng sẽ bị bỏ qua
            EntityAllChange<LocalizedProperty> localizedPropertyEvent = null;
            if(listMapping.Count>0) storeMappingEvent = new EntityAllChange<StoreMapping>(listMapping[0]);
            if(listLocalizedProperty.Count>0) localizedPropertyEvent = new EntityAllChange<LocalizedProperty>(listLocalizedProperty[0]);
            
            _eventPublisher.Publish(currencyEvent, storeMappingEvent, localizedPropertyEvent);
        }

        public Currency GetCurrencyById(int id, bool getFromStaticCache = true)
        {
            if (id <= 0) return null;
            return _cacheWriter.GetById(id, getFromStaticCache, () => { 
                if(getFromStaticCache)
                {
                    var allCurrencies = GetAllCurrenciesCached();
                    return CurrencyForCache.Transform(allCurrencies.FirstOrDefault(p => p.Id == id));
                }
                else
                {
                    return base.GetById(id);
                }
            });
        }

        public Currency GetCurrencyByCode(string currencyCode, bool getFromStaticCache = true)
        {
            if (string.IsNullOrEmpty(currencyCode)) return null;

            return _cacheWriter.GetByCurrencyCode(currencyCode, getFromStaticCache, () =>
            {
                if (getFromStaticCache)
                {
                    var allCurrencies = GetAllCurrenciesCached();
                    return CurrencyForCache.Transform(allCurrencies.FirstOrDefault(p =>
                        string.Equals(currencyCode, p.CurrencyCode, StringComparison.InvariantCultureIgnoreCase)));
                }
                else
                {
                    return _repository.Table.FirstOrDefault(p => p.CurrencyCode == currencyCode);
                }
            });
        }

        public IList<Currency> GetAllCurrencies(bool showHidden = false, int storeId = 0, bool getFromStaticCache = true)
        {
            return _cacheWriter.GetAll(showHidden, storeId, getFromStaticCache, () =>
            {
                if (getFromStaticCache)
                {
                    IEnumerable<CurrencyForCache> allCurrencies = GetAllCurrenciesCached();
                    if (!showHidden) allCurrencies = allCurrencies.Where(p => p.Published);

                    if (storeId > 0)
                    {
                        // lưu ý là vì có liên quan đến store mapping ở đây nên khi có thay đổi ở bảng StoreMapping, cần phải clear
                        // ở những cache như thế này để đảm bảo tính chính xác, đặc biệt là với static cache ( nhất là tình huống
                        // static cache dây chuyền )
                        // Cần hạn chế cache lại từ dữ liệu của hàm này, nếu ko sẽ phải xem xét clear dây chuyền mỗi khi StoreMapping thay đổi
                        return allCurrencies
                            .Select(p => CurrencyForCache.Transform(p))
                            .Where(l => _storeMappingService.Authorize(l, storeId))
                            .ToList();
                    }
                    return allCurrencies.Select(p => CurrencyForCache.Transform(p)).ToList();
                }
                else
                {
                    IQueryable<Currency> allCurrencies = _repository.Table;
                    if (!showHidden) allCurrencies = allCurrencies.Where(p => p.Published);
                    allCurrencies = allCurrencies.OrderBy(p => p.DisplayOrder);
                    if (storeId > 0)
                    {
                        return allCurrencies.ToList()
                            .Where(l => _storeMappingService.Authorize(l, storeId)).ToList();
                    }
                    return allCurrencies.ToList();
                }
            });
        }

        public decimal ConvertCurrency(decimal amount, decimal exchangeRate)
        {
            if (amount == decimal.Zero || exchangeRate == decimal.Zero) return decimal.Zero;
            return amount * exchangeRate;
        }

        public decimal ConvertCurrency(decimal amount, Currency sourceCurrencyCode, Currency targetCurrencyCode)
        {
            if (sourceCurrencyCode.Id == targetCurrencyCode.Id || amount == decimal.Zero) return amount;

            amount = ConvertToPrimaryExchangeRateCurrency(amount, sourceCurrencyCode);
            amount = ConvertFromPrimaryExchangeRateCurrency(amount, targetCurrencyCode);
            return amount;
        }

        /// <summary>
        /// Converts to primary exchange rate currency .
        /// Chuyển đổi amount đơn vị tiền sourceCurrencyCode thành loại tiền tệ cơ sở ( của website )
        /// Cho phép cung cấp primaryExchangeRateCurrency từ bên ngoài nếu đã có dữ liệu này trước đó, tránh việc đọc lại dữ liệu
        /// </summary>
        public decimal ConvertToPrimaryExchangeRateCurrency(decimal amount, Currency sourceCurrencyCode)
        {
            decimal result = amount;
            if (result != decimal.Zero && sourceCurrencyCode.Id != _currencySettings.PrimaryExchangeRateCurrencyId)
            {
                if (sourceCurrencyCode.Rate == decimal.Zero)
                    throw new ResearchException(string.Format("Exchange rate not found for currency [{0}]", sourceCurrencyCode.Name));
                result = result / sourceCurrencyCode.Rate;
            }
            return result;
        }

        public decimal ConvertFromPrimaryExchangeRateCurrency(decimal amount, Currency targetCurrencyCode)
        {
            decimal result = amount;
            if (result != decimal.Zero && targetCurrencyCode.Id != _currencySettings.PrimaryExchangeRateCurrencyId)
            {
                if (targetCurrencyCode.Rate == decimal.Zero)
                    throw new ResearchException(string.Format("Exchange rate not found for currency [{0}]", targetCurrencyCode.Name));
                result = result * targetCurrencyCode.Rate;
            }
            return result;
        }

        // lưu ý : hàm này đc viết hoàn toàn khác với phiên bản trong NOP
        public decimal ConvertToPrimaryStoreCurrency(decimal amount, Currency sourceCurrencyCode)
        {
            if (_currencySettings.PrimaryStoreCurrencyId == _currencySettings.PrimaryExchangeRateCurrencyId)
                return ConvertToPrimaryExchangeRateCurrency(amount, sourceCurrencyCode);
            else
            {
                if (sourceCurrencyCode.Id == _currencySettings.PrimaryStoreCurrencyId || amount == decimal.Zero) return amount;

                var storeCurrency = GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                amount = ConvertToPrimaryExchangeRateCurrency(amount, sourceCurrencyCode);
                amount = ConvertFromPrimaryExchangeRateCurrency(amount, storeCurrency);
                return amount;
            }
        }

        public decimal ConvertFromPrimaryStoreCurrency(decimal amount, Currency targetCurrencyCode)
        {
            if (_currencySettings.PrimaryStoreCurrencyId == _currencySettings.PrimaryExchangeRateCurrencyId)
                return ConvertFromPrimaryExchangeRateCurrency(amount, targetCurrencyCode);
            else
            {
                if (targetCurrencyCode.Id == _currencySettings.PrimaryStoreCurrencyId || amount == decimal.Zero) return amount;

                var storeCurrency = GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                amount = ConvertToPrimaryExchangeRateCurrency(amount, storeCurrency);
                amount = ConvertFromPrimaryStoreCurrency(amount, targetCurrencyCode);
                return amount;
            }
        }

        public IExchangeRateProvider LoadActiveExchangeRateProvider()
        {
            var result = LoadExchangeRateProviderBySystemName(_currencySettings.ActiveExchangeRateProviderSystemName);
            if (result == null)
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptors<IExchangeRateProvider>().FirstOrDefault();
                if (pluginDescriptor != null)
                    result = pluginDescriptor.Instance<IExchangeRateProvider>();
            }
            return result;
        }

        public IExchangeRateProvider LoadExchangeRateProviderBySystemName(string systemName)
        {
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName<IExchangeRateProvider>(systemName);
            if (pluginDescriptor != null)
                return pluginDescriptor.Instance<IExchangeRateProvider>();
            return null;
        }

        public IList<IExchangeRateProvider> LoadAllExchangeRateProviders()
        {
            // bản thân kết quả đã được sắp xếp rồi (_plugins đã đc sắp) nên ta sẽ bỏ qua đoạn OrderBy
            return _pluginFinder.GetPlugins<IExchangeRateProvider>().ToList();
        }

        #endregion
    }
}
