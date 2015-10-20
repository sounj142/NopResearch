using Research.Core.Domain.Directory;
using Research.Core.Infrastructure;
using Research.Core.Interface.Service;
using Research.Services.Events;
using System;

namespace Research.Services.Directory
{
    /// <summary>
    /// Tác vụ chịu trách nhiệm tạo và triệu gọi đối tượng IExchangeRateProvider để gọi ra ben ngoài và cập nhật lại
    /// bảng tỷ giá định ký. Thông tin cấu hình như sau:
    /// + currencySettings.ActiveExchangeRateProviderSystemName: SystemName của plugin IExchangeRateProvider
    /// + currencySettings.AutoUpdateEnabled : Cho biết có cho phép tự động update bảng tỷ giá hay ko. Nếu bằng false thì sẽ ko tự động
    /// update bảng tỷ giá, và UpdateExchangeRateTask sẽ dừng ko làm việc ngay từ đầu hàm 
    /// </summary>
    public class UpdateExchangeRateTask : ITask
    {
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly ISettingService _settingService;

        public UpdateExchangeRateTask(ISettingService settingService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings)
        {
            _settingService = settingService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }

        public void Execute()
        {
            // từ chối thực thi nếu cấu hình ko cho phép tự động cập nhật bảng tỷ giá
            if (!_currencySettings.AutoUpdateEnabled) return;

            DateTime lastUpdateTime = DateTime.FromBinary(_currencySettings.LastUpdateTime);
            lastUpdateTime = DateTime.SpecifyKind(lastUpdateTime, DateTimeKind.Utc); // chuyển thời gian lastUpdateTime sang dạng Utc
            
            // ko cập nhật nếu lần cập nhật gần đây nhất còn quá mới
            if (lastUpdateTime.AddMinutes(_currencySettings.MinMinnuteToUpdateExchangeRate) > DateTime.UtcNow) return;

            // ok, đọc dữ liệu và tiến hành cập nhật
            var defaultCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
            var exchangeRates = _currencyService.GetCurrencyLiveRates(defaultCurrency.CurrencyCode);
            if (exchangeRates == null || exchangeRates.Count == 0) return;

            var allCurrencies = _currencyService.GetAllCurrencies(true, 0, false); // lấy ra tất cả các ngôn ngữ trực tiếp từ database
            bool hasChange = false;
            foreach (var exchangeRate in exchangeRates)
            {
                foreach (var currency in allCurrencies)
                    if (string.Equals(currency.CurrencyCode, exchangeRate.CurrencyCode, StringComparison.InvariantCultureIgnoreCase))
                    {
                        currency.Rate = exchangeRate.Rate;
                        currency.UpdatedOnUtc = DateTime.UtcNow;
                        hasChange = true;
                        break;
                    }
            }

            if (hasChange) _currencyService.Update(allCurrencies); // gọi update , đồng thời phát sinh sự kiện clear cache

            // lưu lại thời gian cập nhật bảng tỷ giá mới nhất
            _currencySettings.LastUpdateTime = DateTime.UtcNow.ToBinary();
            _settingService.SaveSetting(_currencySettings); // lưu lại setting và clear cache
        }
    }
}
