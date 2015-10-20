using System.Collections.Generic;
using Research.Core.Plugins;
using Research.Core.Domain.Directory;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Hệ thống bảng tỷ giá cập nhật định kỳ bằng cách sử dụng 1 plugin bên ngoài. Chúng ta sẽ cung cấp 1 interface IExchangeRateProvider,
    /// mà bản thân interface cũng là 1 IPlugin. Plugin bên ngoài chỉ cần cài đặt interface này là có thể trở thành giao diện lấy bảng
    /// tỷ giá. Bằng cách này sẽ cho phép developer tự cài đặt chức năng lấy bảng tỷ giá phù hợp với nhu cầu của của mình với các đặc điểm
    /// riêng của quốc gia, công ty, ngân hàng, tiền tệ, và dịch vụ lấy tỷ giá mà họ lựa chọn
    /// 
    /// Việc thực thi lấy tỷ giá định kỳ sẽ được thực hiện bởi 1 ScheduleTask, được cài đặt để định kỳ tạo ra thể hiện IExchangeRateProvider,
    /// triệu gọi đến phương thức để lấy bảng tỷ giá do plugin cài đặt ( thường là qua http, tốn nhiều thời gian ). 
    /// Sau khi xong sẽ cập nhật lại bảng tỷ giá trong database và phát ra sự kiện để clear static cache
    /// </summary>
    public partial interface IExchangeRateProvider : IPlugin // plugin thực hiện kết nối ra ngoài để cập nhật bảng tỷ giá cần cài đặt giao diện này
    {

        /// <summary>
        /// Lấy về bảng tỷ giá chuyển đổi từ tất cả các loại tiền tệ mà dịch vụ IExchangeRateProvider có sang loại tiền tệ
        /// exchangeRateCurrencyCode, theo hình thức 1 exchangeRateCurrencyCode = ? loại tiền khác
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Mã của loại tiền tệ cơ sở, chẳng hạn khi ta gửi vào "USD", hàm sẽ trả về tỷ giá chuyển đổi kiểu như 1 USD = ?VND; 1 USD = ? Yen; 1 USD = ? EUR; ...</param>
        /// <returns></returns>
        IList<ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode);
    }
}
