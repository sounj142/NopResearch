using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using MaxMind.GeoIP2.Responses;
using Research.Core;
using Research.Core.Infrastructure;
using Research.Core.Interface.Service;
using System;

namespace Research.Services.Directory
{
    public partial class GeoLookupService : IGeoLookupService
    {
        #region ctor, field, property

        private DatabaseReader _databaseReader;

        public GeoLookupService()
        {
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Lấy thông tin country của 1 ip address
        /// </summary>
        protected virtual CountryResponse GetInformation(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return null;

            try
            {
                // sẽ chỉ tạo duy nhất 1 đối tượng _databaseReader cho mỗi thể hiện IGeoLookupService, tức là per request
                if (_databaseReader == null)
                {
                    var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                    string filePath = webHelper.MapPath("~/App_Data/GeoLite2-Country.mmdb");
                    _databaseReader = new DatabaseReader(filePath);
                }
                return _databaseReader.Country(ipAddress);
            }
            catch (GeoIP2Exception)
            {
                //address is not found
                //do not throw exceptions
                return null;
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                logger.Warning("Không thể load MaxMind.GeoIP2", ex);
                return null;
            }
        }

        #endregion

        #region method

        public virtual string LookupCountryIsoCode(string ipAddress)
        {
            var response = GetInformation(ipAddress);
            if (response != null && response.Country != null)
                return response.Country.IsoCode;
            return string.Empty;
        }

        public string LookupCountryName(string ipAddress)
        {
            var response = GetInformation(ipAddress);
            if (response != null && response.Country != null)
                return response.Country.Name;
            return string.Empty;
        }

        #endregion
    }
}
