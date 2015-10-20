using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Research.Core;
using Research.Core.Caching;
using Research.Core.Data;
using Research.Core.Infrastructure;
using Research.Core.Infrastructure.DependencyManagement;
using Research.Core.Fakes;
using Research.Data;
using Research.Services.Catalog;
using System;
using System.Web;
using System.Linq;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Data.Repositories;
using Research.Web.Framework.Themes;
using Research.Services.Logging;
using Research.Services.Events;
using Research.Services.Caching.Writer;
using Research.Services.Caching.Writer.Implements;
using Research.Services.Configuration;
using Research.Services.Localization;
using Research.Core.Configuration;
using Autofac.Builder;
using System.Collections.Generic;
using System.Reflection;
using Research.Services.Tasks;
using Research.Services.Seo;
using Research.Web.Framework.Mvc.Routes;
using Research.Services.Stores;
using Research.Services.Common;
using Research.Web.Framework.Localization;
using Research.Services.Customers;
using Research.Core.Plugins;
using Research.Services.Directory;
using Research.Services.Authentication;
using Research.Services.Helpers;
using Research.Services.Vendors;
using Research.Services.Security;
using Research.Services.Messages;
using Research.Services.Orders;
using Research.Core.Interface.Service.Orders;
using Research.Services.Cms;
using Research.Core.Interface.Service.Cms;

namespace Research.Web.Framework
{
    public class DependencyRegistrar: IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            // httpcontext and other web component
            builder.Register(c => HttpContext.Current != null ?
                new HttpContextWrapper(HttpContext.Current) : new FakeHttpContext("~/") as HttpContextBase)
                .As<HttpContextBase>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Request)
                .As<HttpRequestBase>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Response)
                .As<HttpResponseBase>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Server)
                .As<HttpServerUtilityBase>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Session)
                .As<HttpSessionStateBase>().InstancePerLifetimeScope();

            //web helper
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerLifetimeScope();
            //user agent helper
            builder.RegisterType<UserAgentHelper>().As<IUserAgentHelper>().InstancePerLifetimeScope();

            // đăng ký tìm assemby trong phạm vi này !
            builder.RegisterControllers(typeFinder.GetAssemblies().ToArray());

            // data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataSettings = dataSettingsManager.LoadSettings();
            builder.RegisterInstance(dataSettings).As<DataSettings>().SingleInstance();
            //builder.Register(c => dataSettingsManager.LoadSettings()).As<DataSettings>();
            builder.Register(x => new EfDataProviderManager(x.Resolve<DataSettings>()))
                .As<BaseDataProviderManager>().InstancePerDependency();

            builder.Register(x => x.Resolve<BaseDataProviderManager>().LoadDataProvider())
                .As<IDataProvider>().InstancePerDependency();
            if (dataSettings != null && dataSettings.IsValid())
            {
                // thử nghiệm chuyển việc thực thi dataProvider.InitConnectionFactory vào cho EfStartUpTask thực hiện
                //var efDataProviderManager = new EfDataProviderManager(dataSettings);
                //var dataProvider = efDataProviderManager.LoadDataProvider();
                //dataProvider.InitConnectionFactory();

                builder.Register<IUnitOfWork>(c => new NopObjectContext(dataSettings.DataConnectionString))
                    .InstancePerLifetimeScope();
                // hiện tại thì UnitOfWork và DbContext là 1
                //builder.Register(c => (IDbContext)c.Resolve<IUnitOfWork>()).As<IDbContext>().InstancePerLifetimeScope();
            }
            else
            {
                throw new Exception("Xay ra loi !");
                //builder.Register<IDbContext>(c => new NopObjectContext(dataSettingsManager.LoadSettings().DataConnectionString))
                //    .InstancePerLifetimeScope();
            }
            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();
            builder.RegisterType<ProductRepository>().As<IProductRepository>().InstancePerLifetimeScope();
            builder.RegisterType<LoggerRepository>().As<ILoggerRepository>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizationRepository>().As<ILocalizationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerRepository>().As<ICustomerRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ActivityLogRepository>().As<IActivityLogRepository>().InstancePerLifetimeScope();

            // cache manager
            // vì có 2 cache cùng interface nên khi gọi .Resolve<ICacheManager>(); mà ko chỉ định rõ tên thì sẽ ra PerRequestCacheManager
            // ( đã test ), khá là ký quái. Nói chung mình nên cài đặt theo hướng dùng 1 giao diện rỗng kế thừa từ ICacheManager
            // cho Perrequest để phân biệt 2 cache, biết rõ cái nào là loại cache gì, tránh nhầm lẫn
            builder.RegisterType<NopNullCache>().As<ICacheManager>().SingleInstance();
            // mỗi đối tượng PerRequestCacheManager sở hữu 1 HttpContextBase riêng nên cần được tạo mới ở mỗi request
            builder.RegisterType<NopNullCache>().As<IPerRequestCacheManager>().InstancePerLifetimeScope();

            // đăng ký các service
            // ( ko hay ho gì khi đặt các interface và class service ở chung 1 namespace, điều này có thể dẫn đến sai lầm
            // sử dụng trực tiếp lớp, thay vì dùng interface thông qua DI ). Tốt nhất nên có 1 namespace riêng cho các interface
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<SettingService>().As<ISettingService>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultLogger>().As<ILogger>().InstancePerLifetimeScope();
            builder.RegisterType<LanguageService>().As<ILanguageService>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizationService>().As<ILocalizationService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerLifetimeScope();
            builder.RegisterType<UrlRecordService>().As<IUrlRecordService>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizedEntityService>().As<ILocalizedEntityService>().InstancePerLifetimeScope();
            builder.RegisterType<StoreService>().As<IStoreService>().InstancePerLifetimeScope();
            builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<StoreMappingService>().As<IStoreMappingService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<CurrencyService>().As<ICurrencyService>().InstancePerLifetimeScope();
            builder.RegisterType<FormsAuthenticationService>().As<IAuthenticationService>().InstancePerLifetimeScope();
            builder.RegisterType<VendorService>().As<IVendorService>().InstancePerLifetimeScope();
            builder.RegisterType<AclService>().As<IAclService>().InstancePerLifetimeScope();
            builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerLifetimeScope();
            builder.RegisterType<PermissionService>().As<IPermissionService>().InstancePerLifetimeScope();
            builder.RegisterType<NewsLetterSubscriptionService>().As<INewsLetterSubscriptionService>().InstancePerLifetimeScope();
            builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerReportService>().As<ICustomerReportService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerAttributeService>().As<ICustomerAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<StateProvinceService>().As<IStateProvinceService>().InstancePerLifetimeScope();
            builder.RegisterType<CountryService>().As<ICountryService>().InstancePerLifetimeScope();
            builder.RegisterType<GeoLookupService>().As<IGeoLookupService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerActivityService>().As<ICustomerActivityService>().InstancePerLifetimeScope();
            builder.RegisterType<GiftCardService>().As<IGiftCardService>().InstancePerLifetimeScope();
            builder.RegisterType<WidgetService>().As<IWidgetService>().InstancePerLifetimeScope();

            
            builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerLifetimeScope();
            builder.RegisterType<ThemeProvider>().As<IThemeProvider>().InstancePerLifetimeScope();
            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();

            //plugins ( khác với Nop là ở đây đã chỉnh lại để tạo ra thể hiện SingleInstance thay vì InstancePerLifetimeScope )
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().SingleInstance();
            builder.RegisterType<OfficialFeedManager>().As<IOfficialFeedManager>().SingleInstance();


            // đăng ký tất cả các consumer với Autofac
            var consumerGenericType = typeof(ICacheConsumer<>);
            var consumerTypes = typeFinder.FindClassesOfType(consumerGenericType); // tìm tất cả các lớp có cài đặt IConsumer trong toàn bộ appdomain
            foreach (var consumerType in consumerTypes)
            {
                // với mỗi lớp, đăng ký lớp đó với tất cả các loại interface IConsumer<X> mà lớp đó cài đặt
                // VD: 1 lớp như ModelCacheEventConsumer sẽ cài đặt rất nhiều IConsumer khác nhau, nên lệnh ở dưới sẽ đăng ký
                // nó với tất cả những interface này
                builder.RegisterType(consumerType).As
                    (
                        consumerType.FindInterfaces((type, criteria) =>
                            type.IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition()), 
                            consumerGenericType)
                    ).InstancePerLifetimeScope();
                // nhận xét: Khi đăng ký nhiều kiểu cùng chịu trách nhiệm cho 1 kiểu, ví dụ :
                // builder.RegisterType<A>().as<I>(), builder.RegisterType<B>().as<I>(), builder.RegisterType<C>().as<I>(), 
                // thì kết quả khi gọi đến Resolver<I>() sẽ trả về A hoặc B, C ( là 1 cái duy nhất, nhưng cơ chế ko rõ ràng, cần phải
                // tìm hiểu thêm ), và lời gọi đến ResolverAll<I>() sẽ trả về 1 mảng 3 đối tượng A, B, C
            }
            // đăng ký EventPublisher và SubscriptionService như nhà sản xuất và bộ tìm kiếm các IConsumer đăng ký nhận sự kiện
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
            builder.RegisterType<SubscriptionService>().As<ISubscriptionService>().SingleInstance();


            // Đăng ký các lớp ghi cache
            builder.RegisterType<LanguageCacheWriter>().As<ILanguageCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<SettingCacheWriter>().As<ISettingCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizationCacheWriter>().As<ILocalizationCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<UrlRecordCacheWriter>().As<IUrlRecordCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizedEntityCacheWriter>().As<ILocalizedEntityCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<StoreCacheWriter>().As<IStoreCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<GenericAttributeCacheWriter>().As<IGenericAttributeCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<StoreMappingCacheWriter>().As<IStoreMappingCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<CurrencyCacheWriter>().As<ICurrencyCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerAndRoleCacheWriter>().As<ICustomerAndRoleCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<SecurityCacheWriter>().As<ISecurityCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerAttributeCacheWriter>().As<ICustomerAttributeCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<StateProvinceCacheWriter>().As<IStateProvinceCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<CountryCacheWriter>().As<ICountryCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<MeasureCacheWriter>().As<IMeasureCacheWriter>().InstancePerLifetimeScope();
            builder.RegisterType<ActivityLogTypeCacheWriter>().As<IActivityLogTypeCacheWriter>().InstancePerLifetimeScope();

            // work context
            builder.RegisterType<WebWorkContext>().As<IWorkContext>().InstancePerLifetimeScope();
            builder.RegisterType<WebStoreContext>().As<IStoreContext>().InstancePerLifetimeScope();
            builder.RegisterType<LanguageUrlStandardize>().As<ILanguageUrlStandardize>().InstancePerLifetimeScope();


            // kiểm tra việc đăng ký động tất cả các ISettings với autofac
            foreach (var settingType in typeFinder.FindClassesOfType<ISettings>())
                if (settingType.GetConstructor(Type.EmptyTypes) != null)
                {
                    //đã chuyển sang dùng cache static cho các isetting theo cặp khóa : kiểu và id store

                    // Ghi chú: Có vấn đề ở đây khi lấy ra đối tượng Setting cho các tác vụ chạy nền. Cú pháp lấy curentstore Id ở dưới sẽ luôn
                    // lấy ra id của store đầu tiên cho các tác vụ nền ( dùng FakeHttpContext ). Do đó, đối tượng setting mà các tác vụ
                    // nền sử dụng sẽ là đối tượng setting của Store đầu tiên, ko phải là đối tượng setting tổng thể với storId=0
                    builder.Register(c => {
                        int storeId = c.Resolve<IStoreContext>().CurrentStore.Id;
                        return c.Resolve<ISettingService>().LoadSetting(settingType, storeId);
                    }).As(settingType).InstancePerLifetimeScope();
                }
            // đăng ký ISettings với kỹ thuật của Nop
            //builder.RegisterSource(new SettingsSource());


        }

        public int Order
        {
            get { return 0; }
        }
    }

    public class SettingsSource : IRegistrationSource
    {
        // khai báo hàm BuildRegistration ở dạng MethodInfo, 1 dạng delegate ?
        private static readonly MethodInfo BuildMethod = typeof(SettingsSource).GetMethod("BuildRegistration",
            BindingFlags.Static | BindingFlags.NonPublic);

        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, 
            IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var ts = service as TypedService;
            if(ts != null && typeof(ISettings).IsAssignableFrom(ts.ServiceType)
                && ts.ServiceType.GetConstructor(Type.EmptyTypes) != null)
            {
                var buildMethod = BuildMethod.MakeGenericMethod(ts.ServiceType);
                yield return (IComponentRegistration)buildMethod.Invoke(null, null);
            }
        }

        /// <summary>
        /// Hàm đăng ký autofact cho kiểu TSettings, với TSettings là ISettings và có 1 hàm tạo ko tham số
        /// Bằng cách sử dụng hàm này, ta có thể gọi đến phiên bản LoadSetting<TSettings>() vốn là phiên bản được cung cấp duy
        /// nhất cho Nop IsSettingService
        /// </summary>
        private static IComponentRegistration BuildRegistration<TSettings>() where TSettings: ISettings, new()
        {
            return RegistrationBuilder.ForDelegate((c, p) => {
                int currentStoreId = c.Resolve<IStoreContext>().CurrentStore.Id;

                return c.Resolve<ISettingService>().LoadSetting<TSettings>(currentStoreId);
            }).InstancePerLifetimeScope()
            .CreateRegistration();
        }
    }
}
