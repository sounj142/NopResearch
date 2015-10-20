using System.Collections.Generic;
using Research.Core.Plugins;
using Research.Core.Interface.Service;
using System.Net;
using System.Xml;
using System;
using System.Globalization;

namespace Research.Plugin.ExchangeRate.McExchange
{
    /// <summary>
    /// Plugin chịu trách nhiệm kết nối ra dịch vụ bên ngoài để lấy về bảng tỷ giá mới nhất.
    /// Ánh xạ IExchangeRateProvider -- McExchangeRateProvider được thực hiện theo cơ chế tìm và tạo thể hiện Plugin thông qua SystemName,
    /// sử dụng IPluginFinder, chứ không dùng DI và Autofac.
    /// 
    /// Vì đc cấu hình trong setting ( currencySettings.ActiveExchangeRateProviderSystemName ) nên mỗi Store thậm chí có thể sử dụng
    /// 1 IExchangeRateProvider riêng
    /// 
    /// 
    /// Do được tạo ko qua đăng ký trực tiếp với DI nên đối tượng IExchangeRateProvider phải được tạo ra với các hàm TryResolve, ResolveUnregistered
    /// </summary>
    public class McExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        /// <summary>
        /// Sử dụng bảng Log để ghi dữ liệu các lần lấy bảng giá ( rất quan trọng để tra cứu sau này nếu có khiếu nại, khi tra ở đây sẽ biết
        /// bảng giá ở thời điểm đó ra sao ). Đồng thời ghi lỗi nếu có lỗi xảy ra
        /// </summary>
        private readonly ILogger _logger;

        public McExchangeRateProvider(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Lấy về bảng tỷ giá chuyển đổi từ tất cả các loại tiền tệ mà dịch vụ IExchangeRateProvider có sang loại tiền tệ
        /// exchangeRateCurrencyCode, theo hình thức 1 exchangeRateCurrencyCode = ? loại tiền khác
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Mã của loại tiền tệ cơ sở, chẳng hạn khi ta gửi vào "USD", hàm sẽ trả về tỷ giá chuyển đổi kiểu như 1 USD = ?VND; 1 USD = ? Yen; 1 USD = ? EUR; ...</param>
        /// <returns></returns>
        public IList<Core.Domain.Directory.ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            var result = new List<Core.Domain.Directory.ExchangeRate>();
            string url = string.Format("http://themoneyconverter.com/rss-feed/{0}/rss.xml", exchangeRateCurrencyCode);

            var request = WebRequest.Create(url);
            using (var response = request.GetResponse())
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());

                var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
                nsmgr.AddNamespace("cf", "http://www.microsoft.com/schemas/rss/core/2005");
                nsmgr.AddNamespace("cfi", "http://www.microsoft.com/schemas/rss/core/2005/internal");

                foreach(XmlNode node in xmlDoc.DocumentElement.FirstChild.ChildNodes)
                    if(string.Equals(node.Name, "item", StringComparison.InvariantCulture))
                    {
                        /*
                                <item>
                                  <title xmlns:cf="http://www.microsoft.com/schemas/rss/core/2005" cf:type="text">AED/NZD</title>
                                  <link>http://themoneyconverter.com/NZD/AED.aspx</link>
                                  <guid>http://themoneyconverter.com/NZD/AED.aspx</guid>
                                  <pubDate>Fri, 20 Feb 2009 08:01:41 GMT</pubDate>
                                  <atom:published xmlns:atom="http://www.w3.org/2005/Atom">2009-02-20T08:01:41Z</atom:published>
                                  <atom:updated xmlns:atom="http://www.w3.org/2005/Atom">2009-02-20T08:01:41Z</atom:updated>
                                  <description xmlns:cf="http://www.microsoft.com/schemas/rss/core/2005" cf:type="html">1 New Zealand Dollar = 1.84499 Arab Emirates Dirham</description>
                                  <category>Middle-East</category>
                                  <cfi:id>32</cfi:id>
                                  <cfi:effectiveId>1074571199</cfi:effectiveId>
                                  <cfi:read>true</cfi:read>
                                  <cfi:downloadurl>http://themoneyconverter.com/NZD/rss.xml</cfi:downloadurl>
                                  <cfi:lastdownloadtime>2009-02-20T08:05:27.168Z</cfi:lastdownloadtime>
                                </item>
                        */

                        try
                        {
                            var rate = new Core.Domain.Directory.ExchangeRate();
                            foreach (XmlNode detailNode in node.ChildNodes)
                            {
                                switch(detailNode.Name)
                                {
                                    case "title":
                                        rate.CurrencyCode = detailNode.InnerText.Substring(0, 3);
                                        break;
                                    case "pubDate":
                                        rate.UpdatedOn = DateTime.Parse(detailNode.InnerText, CultureInfo.InvariantCulture);
                                        break;
                                    case "description":
                                        // 1 New Zealand Dollar = 0.78815 Australian Dollar
                                        string description = detailNode.InnerText;
                                        int x = description.IndexOf('=');
                                        int y = description.IndexOf(' ', x + 2);

                                        string rateText = description.Substring(x + 1, y - x - 1).Trim();
                                        rate.Rate = decimal.Parse(rateText, CultureInfo.InvariantCulture);
                                        break;
                                }
                            }

                            if (!string.IsNullOrEmpty(rate.CurrencyCode)) result.Add(rate);
                        }catch(Exception ex)
                        {
                            _logger.Warning(string.Format("Error parsing currency rates (MC): {0}", ex.Message), ex);
                        }
                    }
                
            }

            return result;
        }
    }
}
