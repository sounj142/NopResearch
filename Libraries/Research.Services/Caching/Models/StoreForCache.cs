using Research.Core.Domain.Stores;

namespace Research.Services.Caching.Models
{
    public class StoreForCache
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the store name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the store URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is enabled
        /// </summary>
        public bool SslEnabled { get; set; }

        /// <summary>
        /// Gets or sets the store secure URL (HTTPS)
        /// </summary>
        public string SecureUrl { get; set; }

        /// <summary>
        /// Gets or sets the comma separated list of possible HTTP_HOST values
        /// </summary>
        public string Hosts { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the company name
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the company address
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// Gets or sets the store phone number
        /// </summary>
        public string CompanyPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the company VAT (used in Europe Union countries)
        /// </summary>
        public string CompanyVat { get; set; }

        public static Store Transform(StoreForCache obj)
        {
            if (obj == null) return null;
            return new Store
            {
                CompanyAddress = obj.CompanyAddress,
                CompanyName = obj.CompanyName,
                CompanyPhoneNumber = obj.CompanyPhoneNumber,
                CompanyVat = obj.CompanyVat,
                DisplayOrder = obj.DisplayOrder,
                Hosts = obj.Hosts,
                Id = obj.Id,
                Name = obj.Name,
                SecureUrl = obj.SecureUrl,
                SslEnabled = obj.SslEnabled,
                Url = obj.Url
            };
        }

        public static StoreForCache Transform(Store obj)
        {
            if (obj == null) return null;
            return new StoreForCache
            {
                CompanyAddress = obj.CompanyAddress,
                CompanyName = obj.CompanyName,
                CompanyPhoneNumber = obj.CompanyPhoneNumber,
                CompanyVat = obj.CompanyVat,
                DisplayOrder = obj.DisplayOrder,
                Hosts = obj.Hosts,
                Id = obj.Id,
                Name = obj.Name,
                SecureUrl = obj.SecureUrl,
                SslEnabled = obj.SslEnabled,
                Url = obj.Url
            };
        }
    }
}
