using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using Research.Core.Caching;
using Research.Core.Data;
using Research.Core;
using Research.Core.Infrastructure;
using Research.Services.Catalog;
using System.Linq.Expressions;
using Research.Core.Domain.Catalog;
using Research.Core.Interface.Service;
using Research.Web.Infrastructure.Cache;
using Research.Services.Events;
using Research.Core.Events;
using Research.Core.Domain.Localization;
using Research.Core.Domain.Configuration;
using Research.Services.Configuration;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Research.Core.Domain.Directory;
using System.Runtime.CompilerServices;
using System.Threading;
using Research.Core.Interface.Data;
using Research.Core.Domain.Customers;
using Research.Services.Customers;
using Research.Core.Interface.Service.Orders;

// cần cẩn trọng khi tạo task mới với Task.Factory.StartNew, và sau đó Task.Wait nó. Có những lúc task được tạo ra được chạy trên chính
// thread của request hiện hành ( khi đó HttpContext.Current khac null, và EngineContext.Current.Resolve<HttpContextBase>() sẽ trả về 1 HttpRequestBase
// hợp lệ ). Trong khi về nguyên lý thì điều này ko hề đảm bảo, có những lúc task đc chạy trên thread khác và HttpContext.Current = null
//, EngineContext.Current.Resolve<HttpContextBase>() là FakeHttpContext

namespace Research.Web.Controllers
{
    // sửa lại để mỗi IXXXService tự cung cấp giao diên cho mình gồm đủ insert, delete, update chứ ko kế thừa từ IBaseService nữa
    // IBaseService sẽ vẫn đc cài đặt bởi BaseService và kế thừa bởi XXXService nhưng sẽ ko được phơi bày ra khi ta sử dụng các IXXXService

    public class HomeController : BasePublicController
    {
        private readonly ICacheManager _cacheManager;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICountryService _countryService;
        private readonly IGiftCardService _giftCardService;
        private readonly ICustomerService _customerService;

        public HomeController(ICacheManager cacheManager, 
            IProductService productService, 
            ISettingService settingService,
            ILanguageService languageService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            ICurrencyService currencyService,
            ICountryService countryService,
            IGiftCardService giftCardService,
            ICustomerService customerService)
        {
            _cacheManager = cacheManager;
            _productService = productService;
            _settingService = settingService;
            _languageService = languageService;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _currencyService = currencyService;
            _countryService = countryService;
            _giftCardService = giftCardService;
            _customerService = customerService;
        }

        
        public ActionResult Index()
        {
            var geoService = EngineContext.Current.Resolve<IGeoLookupService>();
            geoService.LookupCountryName("118.69.128.169");
            geoService.LookupCountryName("1.52.31.232");

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            //DeleteGuestsTask task;
            //if (EngineContext.Current.ContainerManager.TryResolve<DeleteGuestsTask>(out task) ||
            //    (task = EngineContext.Current.ContainerManager.ResolveUnregistered<DeleteGuestsTask>()) != null)
            //{
            //    task.Execute();
            //}

            var customer = _customerService.GetCustomerByEmail("sounj142@gmail.com");

            var listGiftCard = _giftCardService.GetActiveGiftCardsAppliedByCustomer(customer);

            //ViewBag.AllLanguage = _languageService.GetAllLanguages();

            return View();
        }

        public ActionResult BaoLoi()
        {
            return InvokeHttp404();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            var lang = _languageService.GetById(2, false);

            lang.Name = "Tiếng Việt lại tàu";
            _languageService.Update(lang);

            //await Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(1000);
            //    Debug.WriteLine("1 : " + System.Web.HttpContext.Current);
            //    var context = EngineContext.Current.Resolve<HttpContextBase>();
            //    Debug.WriteLine("1 : " + context.GetType().Name);
            //    Debug.WriteLine("1 : " + context.Request.UserAgent);
            //});

            //Debug.WriteLine("Main : " + System.Web.HttpContext.Current);
            //var ct = EngineContext.Current.Resolve<HttpContextBase>();
            //Debug.WriteLine("Main : " + ct.GetType().Name);
            //Debug.WriteLine("Main : " + ct.Request.UserAgent);



            //var currentStore = EngineContext.Current.Resolve<IStoreContext>().CurrentStore;

            //var currencySetting = EngineContext.Current.Resolve<CurrencySettings>();

            //var task = Task.Factory.StartNew(() => {
            //    var storeNow = EngineContext.Current.Resolve<IStoreContext>().CurrentStore;

            //    var cc1 = EngineContext.Current.Resolve<CurrencySettings>();
            //    int a = 2 + 3;
            //});

            //task.Wait();
            
            return View();
        }

        [HttpPost]
        public ActionResult Contact(string name)
        {
            

            return View();
        }
    }

    class QQQ
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Test(string chuoi)
        {
            Debug.WriteLine("Test : " + chuoi);
            Thread.Sleep(3000);
            Debug.WriteLine("Xong : " + chuoi);
        }
    }
    

    public class TestController : Controller
    {
        private readonly ISettingService _service;
        private readonly ILanguageService _languageService;

        public TestController(ISettingService service, ILanguageService languageService)
        {
            _service = service;
            _languageService = languageService;
        }

        public string Test2()
        {
            ////_languageService.GetAllLanguages();
            ////var result = _languageService.GetById(1, true);
            ////result.Name = result.Name;
            ////_languageService.Update(result);

            var setting = _service.GetById(2, false);
            _service.UpdateSetting(setting);

            return "aaaa";
        }

        public string Test()
        {
            //string result = string.Empty;
            //var list = _service.GetAllNoTracking().OrderBy(p => p.Name).ThenBy(p => p.StoreId).ToList();
            //var dictionary = new Dictionary<string, IList<Setting>>();
            //foreach (var seting in list)
            //    if (seting.StoreId == 0)
            //    {
            //        if (!dictionary.ContainsKey(seting.Name))
            //            dictionary.Add(seting.Name, new List<Setting> { seting });
            //    }

            //int totalStore = 10;
            

            //foreach(var ls in dictionary.Values)
            //    for (int i = 1; i < totalStore; i++)
            //    {
                    
            //        var setting = new Setting
            //        {
            //            Name = ls[0].Name,
            //            StoreId = i,
            //            Value = ls[0].Value
            //        };
            //        ls.Add(setting);
            //    }
            //var newDict = new Dictionary<string, Setting>();
            //foreach (var ls in dictionary.Values)
            //    foreach (var setting in ls)
            //        newDict.Add(setting.Name + setting.StoreId, setting);


            //List<string> keyList = (list.Select(p => p.Name).Concat(list.Select(p => new string(p.Name.Reverse().ToArray())))).ToList();

            //int Count = 1000;
            //int found = 0;
            //int storeId;
            //Stopwatch watch = Stopwatch.StartNew();
            //IList<Setting> lsSett;
            //for (int i = 0; i < Count; i++ )
            //{
            //    foreach (string key in keyList)
            //        for (storeId = 0; storeId < totalStore; storeId++)
            //        {
            //            //string s = key + storeId;
            //            //var k = key.Trim().ToLowerInvariant();
            //            if (dictionary.TryGetValue(key, out lsSett))
            //            {
            //                var qq = lsSett.FirstOrDefault(z => z.StoreId == storeId);
            //                if (qq != null) ++found;
            //            }
            //        }     


            //}
            //watch.Stop();
            //result += "Disct: " + watch.ElapsedMilliseconds + "; Found: " + found + " || ";

            //found = 0;
            //watch = Stopwatch.StartNew();
            
            //Setting st;
            //for (int i = 0; i < Count; i++)
            //{
            //    foreach (string key in keyList)
            //        for (storeId = 0; storeId < totalStore; storeId++)
            //        {
            //            if (newDict.TryGetValue(key + storeId, out st))
            //            {
            //                ++found;
            //            }
            //        }
            //}
            //watch.Stop();
            //result += "Disct2: " + watch.ElapsedMilliseconds + "; Found: " + found + " || ";


            //watch = Stopwatch.StartNew();
            //for (int i = 0; i < Count; i++)
            //{
            //    foreach (string key in keyList)
            //    {
            //        int firts = 0, last = list.Count - 1, middle, compare;
            //        Setting item;
            //        while (firts <= last)
            //        {
            //            middle = (firts + last) >> 1;
            //            item = list[middle];
            //            compare = string.Compare(key, item.Name);
            //            if (compare == 0)
            //            {
            //                //if (item.StoreId == storeId)
            //                //    break;
            //                //else if(item.StoreId < storeId)
            //                break;
            //            }
            //            else if (compare < 0) last = middle - 1;
            //            else firts = middle + 1;
            //        }
            //    }
            //}
            //watch.Stop();
            //result += "List: " + watch.ElapsedMilliseconds;

            return "aan";
        }
    }
}