using Research.Core;
using Research.Core.Domain.Localization;
using Research.Core.Interface.Service;
using Research.Web.Framework.Localization;
using System.Web;
using System.Linq;
using System;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Common;
using Research.Core.Fakes;
using Research.Core.Infrastructure;
using Research.Core.Domain.Vendors;
using Research.Core.Domain.Directory;
using Research.Core.Domain.Tax;

namespace Research.Web.Framework
{
    /// <summary>
    /// Đối tượng ngữ cảnh làm việc của ứng dụng web. Thời gian sống là per lifetime scope
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        #region Fields, ctors
        private const string CustomerCookieName = "Research.customer";

        private readonly HttpContextBase _httpContext;
        private readonly ILanguageService _languageService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IUserAgentHelper _userAgentHelper;
        private readonly IAuthenticationService _authenticationService;
        private readonly IVendorService _vendorService;
        private readonly ICurrencyService _currencyService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CommonSettings _commonSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;


        private Customer _cachedCustomer;
        private Customer _originalCustomerIfImpersonated;
        private Language _cachedLanguage;
        private Vendor _cachedVendor;
        private Currency _cachedCurrency;
        private TaxDisplayType? _cachedTaxDisplayType;

        public WebWorkContext(HttpContextBase httpContext,
            ILanguageService languageService,
            LocalizationSettings localizationSettings,
            IGenericAttributeService genericAttributeService,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            ICustomerService customerService,
            IUserAgentHelper userAgentHelper,
            IAuthenticationService authenticationService,
            CommonSettings commonSettings,
            IVendorService vendorService,
            ICurrencyService currencyService, 
            CurrencySettings currencySettings,
            TaxSettings taxSettings)
        {
            _httpContext = httpContext;
            _languageService = languageService;
            _localizationSettings = localizationSettings;
            _genericAttributeService = genericAttributeService;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _customerService = customerService;
            _userAgentHelper = userAgentHelper;
            _authenticationService = authenticationService;
            _commonSettings = commonSettings;
            _vendorService = vendorService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _taxSettings = taxSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Hàm chịu trách nhiệm lấy về ngôn ngữ dựa trên phân đoạn ngôn ngữ đầu tiên trong url ( dùng trong trường hợp hệ thống
        /// được cấu hình sử dụng phân đoạn ngôn ngữ ). Trả về null nếu ko tìm thấy phân đoạn ngôn ngữ, hoặc nếu phân đoạn tìm thấy
        /// ko có ngôn ngữ tương ứng trong database
        /// </summary>
        /// <returns></returns>
        protected virtual Language GetLanguageFromUrl()
        {
            if (_httpContext == null || _httpContext.Request == null) return null;

            var request = _httpContext.Request;

            // khúc này bị sửa để dùng rawUrl thay cho AppRelativeCurrentExecutionFilePath, cái mà sẽ bị rewrite lại để loại bỏ phân đoạn ngôn ngữ
            // điều mà khiến cho nó ko đáng tin cậy để sử dụng ở đây ( cho dù có giả định thứ tự thực hiện, rằng sẽ có 1 lời gọi đến
            // .CurrentLanguage trước routing thì cũng vẫn thấy có vấn đề với nó )
            string rawUrl = request.RawUrl;
            string applicationPath = request.ApplicationPath;
            // nếu rawUrl là link có phân đoạn ngôn ngữ thì nối thêm phân đoạn ngôn ngữ vào phía trước chuỗi url kết quả
            if (!rawUrl.IsLocalizedUrl(applicationPath, true)) return null; // kiểm tra thấy ko có phân đoạn ngôn ngữ

            var seoCode = rawUrl.GetLanguageSeoCodeFromUrl(applicationPath, true);
            if (string.IsNullOrEmpty(seoCode)) return null;

            var language = _languageService.GetAllLanguages()
                .FirstOrDefault(p => string.Equals(p.UniqueSeoCode, seoCode, StringComparison.InvariantCultureIgnoreCase)
                && p.Published);

            if (language != null && _storeMappingService.Authorize(language))
                return language;

            return null;
        }

        /// <summary>
        /// Hàm lấy về ngôn ngữ dựa theo cấu hình của browse máy client
        /// </summary>
        protected virtual Language GetLanguageFromBrowserSettings()
        {
            if (_httpContext == null || _httpContext.Request == null || _httpContext.Request.UserLanguages == null) return null;

            var userLanguage = _httpContext.Request.UserLanguages.FirstOrDefault();
            if (string.IsNullOrEmpty(userLanguage)) return null;

            var language = _languageService.GetAllLanguages()
                .FirstOrDefault(p => string.Equals(p.LanguageCulture, userLanguage, StringComparison.InvariantCultureIgnoreCase)
                && p.Published);
            if (language != null && _storeMappingService.Authorize(language)) return language;

            return null;
        }

        /// <summary>
        /// Hàm trả về cookies được dùng để lưu trữ giá trị Guid của tài khoản Customer Guest hiện hành. Lưu ý là cookies này khác với cookies
        /// authenticate của tài khoản Register. Như vậy, có thể tồn tại đồng thời 2 mục cookies, 1 mục cho tài khoản Guest, 1 mục cho tài khoản
        /// Register. Trong trường hợp có cả 2 thì sẽ ưu tiên dùng tài khoản Register, nhưng sẽ vẫn để nguyên cookies Guest để sử dụng trong 
        /// trường hợp khác. Lưu ý là Customer ứng với Guid chứa trong Guest cookies chỉ có thể là tài khoản Guest. Nếu tài khoản đó là Register
        /// mà lại ko có cookies authenticate thì cookies sẽ bị coi là ko hợp lệ và sẽ bị auto delete
        /// </summary>
        /// <returns></returns>
        protected virtual HttpCookie GetCustomerCookie()
        {
            if (_httpContext == null || _httpContext.Request == null || _httpContext.Request.Cookies == null) return null;
            return _httpContext.Request.Cookies[CustomerCookieName];
        }

        /// <summary>
        /// Hàm thiết lập cookies để lưu giữ Guid của tài khoản Guest
        /// . Nếu customerGuid là empty thì sẽ thực hiện xóa cookies
        /// </summary>
        protected virtual void SetCustomerCookie(Guid customerGuid)
        {
            if (_httpContext == null || _httpContext.Request == null || _httpContext.Request.Cookies == null) return;

            var cookie = new HttpCookie(CustomerCookieName);
            cookie.HttpOnly = true;
            cookie.Value = customerGuid.ToString();
            if (customerGuid == Guid.Empty) 
                cookie.Expires = DateTime.Now.AddMonths(-1);
            else
                cookie.Expires = DateTime.Now.AddMinutes(_commonSettings.GuestGuidCookiesExpiresMinutes);

            _httpContext.Response.Cookies.Remove(CustomerCookieName);
            _httpContext.Response.Cookies.Add(cookie);
        }

        #endregion

        #region Methods

        public virtual Customer CurrentCustomer
        {
            get
            {
                if (_cachedCustomer != null) return _cachedCustomer;

                Customer customer = null;
                // trường hợp là background task ?
                if(_httpContext == null || _httpContext is FakeHttpContext)
                {
                    // nếu _httpContext là null hoặc là fake thì ta sẽ xác định rằng đây là request của BackgroundTask
                    // và sẽ lấy tài khoản dành cho BackgroundTask ra để sử dụng

                    // ==> với cách đối xử như thế này, chỉ cần _httpContext là FAKE thì sẽ mặc nhiên lấy tài khoản BackgroundTask ra sử dụng
                    //. Như thế thì sẽ ko thể unittest cho các trương hợp khác ở bên dưới ? Hay là để unittest có thể đi được xuống phần code 
                    // dưới, dữ liệu mock của unit test sẽ phải ko có tài khoản BackgroundTask ???

                    customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.BackgroundTask);

                    // kiểm tra riêng để ném ngoại lệ cho trường hợp _httpContext == null ( thực ra trường hợp này sẽ ko bao giờ xảy ra ? )
                    if (_httpContext == null && (customer == null || customer.Deleted || !customer.Active))
                        throw new ResearchException("WebWorkContext.CurrentCustomer: BackgroundTask customer is not valid");
                }

                // trường hợp là search enginer, sử dụng customer SearchEngine
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    if(_userAgentHelper.IsSearchEngine())
                        customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.SearchEngine);
                }

                // là người dùng đã đăng ký ?
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    try
                    {
                        customer = _authenticationService.GetAuthenticatedCustomer();
                    }catch(AuthenticationDangerousException)
                    {
                        // xảy ra ngoại lệ => cookies hiện hành đang chứa 1 tài khoản ko hợp lệ, có thể là do ko tìm thấy tài khoản, do mật khẩu sai
                        // hoặc là do tài khoản bị xóa, bị ban. Trong trường hợp này ta sẽ cưỡng ép Sigout để xóa cookies lỗi, và sẽ tiếp tục các
                        // xử lý khác ở bên dưới
                        _authenticationService.SignOut();
                    }
                }

                // hiểu nôm na là 1 tài khoản sử dụng 1 tài khoản khác làm đại diện cho nó. 1 tài khoản customer có thể khai báo 1 property
                // tên ImpersonatedCustomerId trong GenericAttribute để chứa Id của 1 tài khoản khác. Và tài khoản tương ứng với 
                // ImpersonatedCustomerId nếu có sẽ là thứ được trả về  bởi IWebContext.CurrentCustomer

                //impersonate user if required (currently used for 'phone order' support)
                if (customer != null && !customer.Deleted && customer.Active)
                {
                    // KHÁC Ở ĐÂY : thêm tham số là store Id hiện hành
                    int? impersonatedCustomerId = customer.GetAttribute<int?>(SystemCustomerAttributeNames.ImpersonatedCustomerId, 
                        _storeContext.CurrentStore.Id);
                    if(impersonatedCustomerId.HasValue && impersonatedCustomerId.Value>0)
                    {
                        var impersonatedCustomer = _customerService.GetById(impersonatedCustomerId.Value);
                        if (impersonatedCustomer != null && !impersonatedCustomer.Deleted && impersonatedCustomer.Active)
                        {
                            //set impersonated customer
                            _originalCustomerIfImpersonated = customer; // lưu trữ lại customer nguyên bản
                            customer = impersonatedCustomer;
                        }
                    }
                }

                //load guest customer
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    var customerCookie = GetCustomerCookie();
                    if(customerCookie != null && !string.IsNullOrEmpty(customerCookie.Value))
                    {
                        Guid customerGuid;
                        if(Guid.TryParse(customerCookie.Value, out customerGuid))
                        {
                            //this customer (from cookie) should not be registered
                            var guestCustomer = _customerService.GetCustomerByGuid(customerGuid);
                            if (guestCustomer != null && !guestCustomer.Deleted && guestCustomer.Active && 
                                //guestCustomer.IsGuest() &&
                                !guestCustomer.IsRegistered())
                                customer = guestCustomer;
                        }
                    }
                }

                // tạo ra tài khoản guest nếu đi đến đây rồi mà vẫn ko tìm đc customer
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    //  LƯU Ý : cách tạo tài khoản Guest và lưu trữ 1 cookies chứa Guid của tài khoản lâu dài trên máy người dùng để ngăn chặn
                    // việc tạo tài khoản Guest nhiều lần chỉ tốt trong kịch bản thông thường. Trong kịch bản tấn công, kẻ tấn công có thể sử dụng
                    // 1 chương trình no cookies hoặc tự động clear cookies để tự động kết nối đến máy chủ nhiều lần. Mỗi lần như vậy sẽ có
                    // 1 tài khoản Guest được tạo ra, và chả mấy chốc mà bảng Customer tràn ngập những Guest "rác". Liệu con số int.Maxint của
                    // bảng Customer có đúng vững trước kiểu tấn công này ?
                    customer = _customerService.InsertGuestCustomer();
                }

                // ghi cookies 
                if (!customer.Deleted && customer.Active)
                {
                    SetCustomerCookie(customer.CustomerGuid);
                    _cachedCustomer = customer;
                }

                return _cachedCustomer;
            }
            // thao tác set cho phép thiết lập trực tiếp 1 Customer làm customer hiện hành, nhưng chỉ thiết lập ở "mức guest" với guet cookies
            set
            {
                SetCustomerCookie(value.CustomerGuid);
                _cachedCustomer = value;
            }
        }

        public virtual Customer OriginalCustomerIfImpersonated
        {
            get
            {
                if (_cachedCustomer == null)
                {
                    var customer = CurrentCustomer;
                }
                return _originalCustomerIfImpersonated;
            }
        }

        public virtual Vendor CurrentVendor
        {
            get
            {
                if (_cachedVendor != null) return _cachedVendor;

                var currentCustomer = this.CurrentCustomer;
                if (currentCustomer == null) return null;

                // lấy ra đối tượng vendor liên kết với customer hiện hành nếu có
                var vendor = _vendorService.GetById(currentCustomer.VendorId);
                if (vendor != null && vendor.Active && !vendor.Deleted)
                    _cachedVendor = vendor;

                return _cachedVendor;
            }
        }

        /// <summary>
        /// Đã sửa lại để lưu trữ 1 property ngôn ngữ riêng cho giao diện admin, 1 property ngôn ngữ riêng cho giao diện khách hàng
        /// </summary>
        public virtual Language WorkingLanguage 
        {
            get
            {
                if (_cachedLanguage != null) return _cachedLanguage;

                int currentStoreId = _storeContext.CurrentStore.Id;
                Language detectedLanguage = null;
                var currentCustomer = this.CurrentCustomer;

                // ưu tiên 1: lấy language từ url
                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                    detectedLanguage = GetLanguageFromUrl();

                bool languageAutomaticallyDetected = false;
                // ưu tiên 2: lấy language từ browser setting
                // nhưng chúng ta sẽ chỉ lấy nó 1 lần duy nhất đối với mỗi customer ( ? )
                if (detectedLanguage == null && _localizationSettings.AutomaticallyDetectLanguage)
                {
                    // thay vì dùng session, ta sẽ dùng cách lưu trữ 1 giá trị bool vào bảng GenericAttribute để ghi nhận rằng ta
                    // đã thực hiện xong việc tự động lấy ngôn ngữ từ cấu hình trình duyệt, để tất cả những lần sau ta ko cần phải
                    // lặp lại thao tác này nữa. 
                    // Tuy nhiên sẽ cần có cách để hủy bỏ giá trị này đi sau khi kết thúc phiên làm việc ? Hay là ta cứ để nguyên nó đó
                    // và việc phát hiện ngôn ngữ dựa trên browser chỉ được thực hiện duy nhất 1 lần trên suốt "vòng đời" của tài khoản 
                    // customer ??

                    if (!currentCustomer.GetAttribute<bool>(SystemCustomerAttributeNames.LanguageAutomaticallyDetected,
                        _genericAttributeService, currentStoreId))
                    {
                        detectedLanguage = GetLanguageFromBrowserSettings();
                        if (detectedLanguage != null)
                        {
                            // nếu xác định được ngôn ngữ thông qua cấu hình browser, ghi nhận đã thực hiện thao tác ấy để tránh lặp lại
                            _genericAttributeService.SaveAttribute<bool>(currentCustomer,
                                SystemCustomerAttributeNames.LanguageAutomaticallyDetected, true, currentStoreId);
                            languageAutomaticallyDetected = true;
                        }
                    }
                }
                // tùy theo giao diện admin hay khách hàng mà chọn tên property language phù hợp
                string genericLanguageName = IsAdmin ? 
                    SystemCustomerAttributeNames.AdminLanguageId : SystemCustomerAttributeNames.LanguageId;


                // nếu phát hiện ngôn ngữ ở 2 bước trên thành công, lưu lại ngôn ngữ vào bảng genericAttribute làm ngôn ngữ cho người
                // dùng hiện hành nếu kiểm tra thấy có sự thay đổi ngôn ngữ
                if (detectedLanguage != null)
                {
                    if (currentCustomer.GetAttribute<int>(genericLanguageName,
                        _genericAttributeService, currentStoreId) != detectedLanguage.Id)
                    {
                        _genericAttributeService.SaveAttribute<int>(currentCustomer, genericLanguageName,
                            detectedLanguage.Id, currentStoreId);
                    }

                    // nếu ngôn ngữ là được lấy ra từ cấu hình trình duyệt ( thao tác vốn chỉ được thực hiện duy nhất 1 lần ),
                    // kiểm tra ngôn ngữ dùng cho bộ giao diện còn lại, nếu khác thì cập nhật lại luôn
                    if (languageAutomaticallyDetected)
                    {
                        string anotherLanguage = IsAdmin ?
                            SystemCustomerAttributeNames.LanguageId : SystemCustomerAttributeNames.AdminLanguageId;
                        if (currentCustomer.GetAttribute<int>(anotherLanguage,
                            _genericAttributeService, currentStoreId) != detectedLanguage.Id)
                        {
                            _genericAttributeService.SaveAttribute<int>(currentCustomer, anotherLanguage,
                            detectedLanguage.Id, currentStoreId);
                        }
                    }
                }
                else
                {
                    var allLanguages = _languageService.GetAllLanguages(storeId: currentStoreId);

                    // xác định ngôn ngữ hiện hành dựa trên thông tin lưu trữ trong generic prop Customer.LanguageId, đồng thời phải là
                    // ngôn ngữ được current store hỗ trợ
                    int languageId = currentCustomer.GetAttribute<int>(genericLanguageName,
                        _genericAttributeService, currentStoreId);
                    detectedLanguage = allLanguages.FirstOrDefault(p => p.Id == languageId);

                    // nếu ko thấy ngôn ngữ phù hợp thì lấy ra ngôn ngữ đầu tiên làm ngôn ngữ cho request
                    if (detectedLanguage == null) detectedLanguage = allLanguages.FirstOrDefault();
                    if (detectedLanguage == null) detectedLanguage = _languageService.GetAllLanguages().FirstOrDefault();
                }

                _cachedLanguage = detectedLanguage;
                return _cachedLanguage;
            }

            // thao tác set sẽ luôn kiểm tra để đảm bảo ngôn ngữ là đc hỗ trợ bởi Store, sau đó sẽ thiết lập lại _cachedLanguage về
            // ngôn ngữ mới luôn chứ ko ngớ ngẩn đi thiết lập _cachedLanguage về null
            set
            {
                if (value == null || value.Id <= 0) return;
                int currentStoreId = _storeContext.CurrentStore.Id;

                var allLanguages = _languageService.GetAllLanguages(storeId: currentStoreId);

                // kiểm tra lại để bảo đảm current Store hỗ trợ ngôn ngữ được yêu cầu
                var language = allLanguages.FirstOrDefault(l => l.Id == value.Id);
                if (language != null && _storeMappingService.Authorize(language))
                {
                    // tùy theo giao diện admin hay khách hàng mà chọn tên property language phù hợp
                    string genericLanguageName = IsAdmin ?
                        SystemCustomerAttributeNames.AdminLanguageId : SystemCustomerAttributeNames.LanguageId;
                    // lưu lại ngôn ngữ mới cho người dùng hiện hành
                    _genericAttributeService.SaveAttribute<int>(this.CurrentCustomer, genericLanguageName,
                            language.Id, currentStoreId);

                    _cachedLanguage = language;
                }
            }
        }

        public virtual Currency WorkingCurrency
        {
            get
            {
                if (_cachedCurrency != null) return _cachedCurrency;

                // đối với giao diện Admin, chúng ta sẽ có 1 mã id trong bảng setting qui định loại tiền tệ được sử dụng. Do đó, chúng ta
                // sẽ có 1 mã tiền tệ chung (storeId=0), và thậm chí có thể định nghĩa mã tiền tệ riêng cho mỗi store
                // . Loại tiền tệ hiện hành của giao diện Admin đc xác định theo cách đó, vì thế sẽ ko có combobox đổi loại tiền tệ trong
                // giao diện admin
                if(this.IsAdmin)
                {
                    var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                    if(primaryStoreCurrency != null)
                    {
                        _cachedCurrency = primaryStoreCurrency;
                        return _cachedCurrency;
                    }
                    // trường hợp primaryStoreCurrency == null, ta vẫn cho code chạy xuống dưới để lấy về mã tiền tệ bên giao diện khách
                    // hàng để dùng cho giao diện admin, tuy nhiên đây là điều hầu như ko xảy ra, dữ liệu đúng sẽ luôn đảm bảo code
                    // dừng ở đoạn mã bên trên
                }
                
                var currentStore = _storeContext.CurrentStore;
                var currentCustomer = this.CurrentCustomer;
                // lấy về tất cả các tiền tệ mà store hiện hành hỗ trợ
                var allCurrencies = _currencyService.GetAllCurrencies(storeId: currentStore.Id); 

                // lấy về mã currency được lưu trữ trong bảng generic attribute của customer hiện hành
                int currencyId = currentCustomer.GetAttribute<int>(SystemCustomerAttributeNames.CurrencyId, 
                    _genericAttributeService, currentStore.Id);
                var currency = allCurrencies.FirstOrDefault(p => p.Id == currencyId);

                // nếu ko có, dùng mã tiền tệ của ngôn ngữ hiện hành ( mỗi ngôn ngữ sẽ qui định mã tiền tệ của nó, chẳng hạn tiếng anh
                // xài USD, tiếng việt xài VND ). Quyết định loại tiền tệ dựa trên ngôn ngữ sẽ là quyết định cần đc ưu tiên nhất
                if (currency == null)
                {
                    var workingLanguage = this.WorkingLanguage;
                    if (workingLanguage.DefaultCurrencyId > 0)
                        currency = allCurrencies.FirstOrDefault(p => p.Id == workingLanguage.DefaultCurrencyId);
                }

                // nếu tiếp tục ko thấy, dùng curency đầu tiên làm tiền tệ hiện hành
                if(currency == null)
                {
                    currency = allCurrencies.FirstOrDefault();
                }
                // vẫn tiếp tục ko thấy, xóa bỏ ràng buộc store để lấy currency đầu tiên của hệ thống nếu có
                if(currency == null)
                {
                    currency = _currencyService.GetAllCurrencies().FirstOrDefault();
                }

                _cachedCurrency = currency;
                return _cachedCurrency;
            }
            // cho phép thiết lập currency mặc định cho người dùng hiện hành. Nếu set = null thì sẽ hủy bỏ thiết lập ( khi đó currency
            // mặc định sẽ là currency default của ngôn ngữ, hoặc currency đầu tiên trong danh sách currency tìm được
            // việc thiết lập này chỉ có ý nghĩa trên giao diện khách hàng ( combobox thay đổi tiền tệ )
            set
            {
                int currencyId = value != null ? value.Id : 0;

                // việc thiết lập này có thể sai, khi mà ngôn ngữ đc lưu vào ko đc hỗ trợ bởi store hiện hành. Việc kiểm tra điều này sẽ
                // được bảo đảm bởi thao tác get khi chúng ta thiết lập lại _cachedCurrency == null
                _genericAttributeService.SaveAttribute(this.CurrentCustomer, SystemCustomerAttributeNames.CurrencyId,
                    currencyId, _storeContext.CurrentStore.Id);

                _cachedCurrency = null; 
            }
        }

        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        public virtual TaxDisplayType TaxDisplayType
        {
            get
            {
                if (_cachedTaxDisplayType != null) return _cachedTaxDisplayType.Value;

                TaxDisplayType taxDisplayType;
                if(_taxSettings.AllowCustomersToSelectTaxDisplayType)
                {
                    taxDisplayType = (TaxDisplayType)this.CurrentCustomer.GetAttribute<int>(
                        SystemCustomerAttributeNames.TaxDisplayTypeId, _genericAttributeService, _storeContext.CurrentStore.Id);
                }else
                {
                    taxDisplayType = _taxSettings.TaxDisplayType;
                }

                if (!Enum.IsDefined(typeof(TaxDisplayType), (int)taxDisplayType)) taxDisplayType = Core.Domain.Tax.TaxDisplayType.ExcludingTax;

                _cachedTaxDisplayType = taxDisplayType;
                return _cachedTaxDisplayType.Value;
            }
            set
            {
                if (!_taxSettings.AllowCustomersToSelectTaxDisplayType) return;
                if (!Enum.IsDefined(typeof(TaxDisplayType), (int)value)) return;

                _genericAttributeService.SaveAttribute(this.CurrentCustomer, SystemCustomerAttributeNames.TaxDisplayTypeId,
                    (int)value, _storeContext.CurrentStore.Id);

                _cachedTaxDisplayType = value;
            }
        }

        public virtual bool IsAdmin { get; set; }

        #endregion

    }
}