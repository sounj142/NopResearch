using System.Collections.Generic;
using Research.Core.Domain.Directory;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Service chịu trách nhiệm xử lý các thông tin tiền tệ trong bảng Currency, và các thông tin tỷ giá tiền tệ
    /// 
    /// Ko giống như Nop chỉ cache per request, chúng ta sẽ cache static để cache tất cả các lọai tiền tệ, và dùng cache per request
    /// để cache lại theo id/cache danh sách theo tình trạng hidden
    /// 
    /// Cần cẩn trọng lưu ý đến nguồn dữ liệu khi thực hiện các thao tác insert, delete, update. Sẽ an toàn nhất nếu nguồn dữ liệu lấy trực
    /// tiếp từ database ( dữ liệu lấy từ static cache sẽ có thể có lỗi nếu có property khóa ngoại )
    /// </summary>
    public partial interface ICurrencyService
    {
        /// <summary>
        /// Lấy về bảng tỷ giá tiền tệ mới nhất. Hàm sẽ tạo ra đối tượng IExchangeRateProvider và IExchangeRateProvider thường gọi ra bên ngoài lấy về bảng tỷ giá mới nhất
        /// . Cần cẩn trọng khi gọi hàm này vì đây là thao tác mất thời gian.
        /// Bảng tỷ giá sẽ đc biểu diễn ở dạng 1 exchangeRateCurrencyCode = ? tiền tệ khác
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Mã tiền tệ được lấy làm mốc tỷ giá, VD: "USD"</param>
        IList<ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode);

        /// <summary>
        /// Khác với cài đặt trong NOP khi mà NOP chỉ xóa dữ liệu tại bảng Currency và để lại 1 mớ rác tại các bảng LocalizedProperty,
        /// StoreMapping, ( và có thể còn các bảng khác nữa ). Cài đặt Delete ở đây sẽ nỗ lực xóa hết dữ liệu liên quan tại các bảng
        /// này, cài đặt các thao tác xóa trong phạm vi của 1 transaction
        /// </summary>
        void Delete(Currency entity);

        /// <summary>
        /// Lấy Currency theo id, luôn luôn cache per request, nhưng cho phép chọn lựa lấy dữ liệu từ static cache hoặc trực tiếp từ database
        /// thông qua cờ
        /// </summary>
        Currency GetCurrencyById(int id, bool getFromStaticCache = true);

        /// <summary>
        /// Lấy về ngôn ngữ theo mã ngôn ngữ. luôn cache perRequest. 
        /// Cho phép lựa chọn lấy dữ liệu từ static cache hoặc trực tiếp từ database
        /// </summary>
        Currency GetCurrencyByCode(string currencyCode, bool getFromStaticCache = true);

        /// <summary>
        /// Lấy tất cả Currency thỏa cờ showHidden và phải hỗ trợ 1 storeId nào đó ( 0: lấy tất cả ko phân biệt store )
        /// Chỉ cache per request, sau khi vào CommonController, ở đoạn code cần hiển thị ra view sẽ cache riêng 1 lần nữa
        /// theo static cache
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <param name="getFromStaticCache">Cho biết sẽ lấy dữ liệu từ static cache hay trực tiếp từ database</param>
        IList<Currency> GetAllCurrencies(bool showHidden = false, int storeId = 0, bool getFromStaticCache = true);

        void Insert(Currency entity);

        void Update(Currency entity);

        void Update(IList<Currency> entities);

        /// <summary>
        /// Hàm chuyển đổi tiền tệ theo 1 tỷ giá chuyển đổi cho trước
        /// </summary>
        decimal ConvertCurrency(decimal amount, decimal exchangeRate);

        /// <summary>
        /// Hàm chuyển 1 số lượng amount tiền tệ thuộc loại sourceCurrencyCode sang loại targetCurrencyCode
        /// </summary>
        decimal ConvertCurrency(decimal amount, Currency sourceCurrencyCode, Currency targetCurrencyCode);

        /// <summary>
        /// Converts to primary exchange rate currency .
        /// Chuyển đổi amount đơn vị tiền sourceCurrencyCode thành loại tiền tệ cơ sở ( của website )
        /// </summary>
        decimal ConvertToPrimaryExchangeRateCurrency(decimal amount, Currency sourceCurrencyCode);

        /// <summary>
        /// Chuyển đổi amount đơn vị tiền tệ cơ sở ( của website ) thành loại tiền targetCurrencyCode
        /// </summary>
        decimal ConvertFromPrimaryExchangeRateCurrency(decimal amount, Currency targetCurrencyCode);

        /// <summary>
        /// Converts to primary store currency .
        /// Chuyển đổi amount đơn vị tiền sourceCurrencyCode thành loại tiền tệ cơ sở của store
        /// </summary>
        decimal ConvertToPrimaryStoreCurrency(decimal amount, Currency sourceCurrencyCode);

        /// <summary>
        /// Converts from primary store currency.
        /// Chuyển đổi amount đơn vị tiền tệ cơ sở của store thành loại tiền targetCurrencyCode
        /// </summary>
        decimal ConvertFromPrimaryStoreCurrency(decimal amount, Currency targetCurrencyCode);

        /// <summary>
        /// Tạo ra và trả về đối tượng IExchangeRateProvider hiện hành ( được code trong plugin, và được cấu hình SystemName
        /// trong bảng Setting : currencySettings.ActiveExchangeRateProviderSystemName ). Trường hợp ko tìm thấy, thay thế bằng 
        /// IExchangeRateProvider đầu tiên tìm được nếu có
        /// </summary>
        IExchangeRateProvider LoadActiveExchangeRateProvider();

        /// <summary>
        /// Tạo ra và trả về đối tượng IExchangeRateProvider thuộc về plugin với systemName cho trước
        /// </summary>
        IExchangeRateProvider LoadExchangeRateProviderBySystemName(string systemName);

        /// <summary>
        /// Tạo ra đối tượng của tất cả các IExchangeRateProvider tồn tại trong hệ thống, sẽ ko tạo thể hiện của đối tượng thuộc
        /// plugin ko dc install ?
        /// </summary>
        IList<IExchangeRateProvider> LoadAllExchangeRateProviders();
    }
}
