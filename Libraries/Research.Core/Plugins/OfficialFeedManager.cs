using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Research.Core.Plugins
{
    /// <summary>
    /// Cho kết nối với máy chủ Nop để tìm kiếm các plugin đang có
    /// </summary>
    public partial class OfficialFeedManager : IOfficialFeedManager
    {
        private const string GET_CATEGORIES_FEED_URL = "http://www.nopcommerce.com/extensionsxml.aspx?getCategories=1";
        private const string GET_VERSIONS_FEED_URL = "http://www.nopcommerce.com/extensionsxml.aspx?getVersions=1";
        private const string GET_PLUGINS_FEED_URL = "http://www.nopcommerce.com/extensionsxml.aspx?category={0}&version={1}&price={2}&pageIndex={3}&pageSize={4}&searchTerm={5}";
        private const int TIME_OUT = 5000;

        public virtual async Task<IList<OfficialFeedCategory>> GetCategories()
        {
            return await Task.Factory.StartNew<IList<OfficialFeedCategory>>(() =>
            {
                var result = new List<OfficialFeedCategory>();

                CreateWebRequestAndProcessResponse(GET_CATEGORIES_FEED_URL, (xmlDoc) =>
                {
                    foreach (XmlNode node in xmlDoc.SelectNodes(@"//categories/category"))
                    {
                        string id = node.SelectNodes(@"id")[0].InnerText;
                        string parentCategoryId = node.SelectNodes(@"parentCategoryId")[0].InnerText;
                        string name = node.SelectNodes(@"name")[0].InnerText;

                        result.Add(new OfficialFeedCategory
                        {
                            Id = int.Parse(id),
                            Name = name,
                            ParentCategoryId = int.Parse(parentCategoryId)
                        });
                    }
                });

                return result;
            });
        }

        public virtual async Task<IList<OfficialFeedVersion>> GetVersions()
        {
            return await Task.Factory.StartNew<IList<OfficialFeedVersion>>(() =>
            {
                var result = new List<OfficialFeedVersion>();

                CreateWebRequestAndProcessResponse(GET_VERSIONS_FEED_URL, (xmlDoc) =>
                {
                    foreach (XmlNode node in xmlDoc.SelectNodes(@"//versions/version"))
                    {
                        string id = node.SelectNodes(@"id")[0].InnerText;
                        string name = node.SelectNodes(@"name")[0].InnerText;

                        result.Add(new OfficialFeedVersion
                        {
                            Id = int.Parse(id),
                            Name = name
                        });
                    }
                });

                return result;
            });
        }

        public virtual async Task<IPagedList<OfficialFeedPlugin>> GetAllPlugins(int categoryId = 0, int versionId = 0, 
            int price = 0, string searchTerm = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return await Task.Factory.StartNew<IPagedList<OfficialFeedPlugin>>(() =>
            {
                var result = new List<OfficialFeedPlugin>();
                string requestUrl = string.Format(GET_PLUGINS_FEED_URL, categoryId, versionId, price, pageIndex, pageSize,
                    HttpUtility.UrlEncode(searchTerm));
                int totalRecord = 0;

                CreateWebRequestAndProcessResponse(requestUrl, (xmlDoc) =>
                {
                    foreach (XmlNode node in xmlDoc.SelectNodes(@"//extensions/extension"))
                    {
                        var name = node.SelectNodes(@"name")[0].InnerText;
                        var url = node.SelectNodes(@"url")[0].InnerText;
                        var pictureUrl = node.SelectNodes(@"picture")[0].InnerText;
                        var category = node.SelectNodes(@"category")[0].InnerText;
                        var versions = node.SelectNodes(@"versions")[0].InnerText;
                        var priceValue = node.SelectNodes(@"price")[0].InnerText;

                        result.Add(new OfficialFeedPlugin
                        {
                            Name = name,
                            Category = category,
                            PictureUrl = pictureUrl,
                            Price = priceValue,
                            SupportedVersions = versions,
                            Url = url
                        });
                    }

                    var nodes = xmlDoc.SelectNodes(@"//totalRecords");
                    if (nodes.Count > 0)
                        totalRecord = int.Parse(nodes[nodes.Count - 1].SelectNodes(@"value")[0].InnerText);
                });

                return new PagedList<OfficialFeedPlugin>(result, pageIndex, pageSize, totalRecord);
            });
        }

        private void CreateWebRequestAndProcessResponse(string url, Action<XmlDocument> processResponseFunc)
        {
            var request = WebRequest.Create(url);
            request.Timeout = 5000;
            using (var response = request.GetResponse())
            {
                var reader = new StreamReader(response.GetResponseStream());
                string responseFromServer = reader.ReadToEnd();

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseFromServer);

                processResponseFunc(xmlDoc);
            }
        }
    }
}
