using System;

namespace Research.Core.Domain.Directory
{
    /// <summary>
    /// Miêu tả cho 1 tỷ giá chuyển đổi tiền tệ
    /// </summary>
    public partial class ExchangeRate
    {
        /// <summary>
        /// Creates a new instance of the ExchangeRate class
        /// </summary>
        public ExchangeRate()
        {
            CurrencyCode = string.Empty;
            Rate = 1.0m;
        }

        /// <summary>
        /// The three letter ISO code for the Exchange Rate, e.g. USD
        /// Mã của loại tiền tệ
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The conversion rate of this currency from the base currency
        /// Tỷ giá chuyển đổi giữa mã tiền tệ này ( CurrencyCode ) so với loại tiền tệ được chọn làm cơ sở
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// When was this exchange rate updated from the data source (the internet data xml feed)
        /// Ngày cập nhật tỷ giá này
        /// </summary>
        public DateTime UpdatedOn { get; set; }


        /// <summary>
        /// Format the rate into a string with the currency code, e.g. "USD 0.72543"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", this.CurrencyCode, this.Rate);
        }
    }
}
