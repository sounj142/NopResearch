using Research.Core.Configuration;

namespace Research.Core.Domain.Directory
{
    public class CurrencySettings : ISettings
    {
        public bool DisplayCurrencyLabel { get; set; }

        /// <summary>
        /// Mã tiền tệ mặc định của store, mỗi store có thể tự định nghĩa 1 mã tiền tệ mặc định riêng
        /// </summary>
        public int PrimaryStoreCurrencyId { get; set; }

        /// <summary>
        /// Mã tiền tệ tiêu chuẩn mặc định của toàn bộ website
        /// </summary>
        public int PrimaryExchangeRateCurrencyId { get; set; }
        public string ActiveExchangeRateProviderSystemName { get; set; }

        /// <summary>
        /// Cờ cho biết có tự động update lại bảng tỷ giá hay ko. Vì đây là cờ mức website nên chỉ nên có 1 cờ duy nhất mức global storeId = 0, 
        /// ko nên có cờ mức Store
        /// </summary>
        public bool AutoUpdateEnabled { get; set; }


        // thôi kệ, đã lỡ clear cache cho currency rồi thì clear luôn cho setting, lấy dữ liệu mới luôn cho nó nóng sốt
        /// <summary>
        /// Thời gian của lần cuối cập nhật lại bảng tỷ giá tiền tệ
        /// </summary>
        public long LastUpdateTime { get; set; }

        /// <summary>
        /// (bổ sung). Qui định thời gian tối thiểu tính theo phút để cập nhật lại bảng tỷ giá thông qua UpdateExchangeRateTask
        /// </summary>
        public int MinMinnuteToUpdateExchangeRate { get; set; }
    }
}
