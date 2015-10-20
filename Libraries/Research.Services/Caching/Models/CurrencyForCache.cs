using Research.Core.Domain.Directory;
using System;

namespace Research.Services.Caching.Models
{
    public class CurrencyForCache
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the currency code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the rate
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the display locale
        /// </summary>
        public string DisplayLocale { get; set; }

        /// <summary>
        /// Gets or sets the custom formatting
        /// </summary>
        public string CustomFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }


        public static Currency Transform(CurrencyForCache obj)
        {
            if (obj == null) return null;
            return new Currency
            {
                CreatedOnUtc = obj.CreatedOnUtc,
                CurrencyCode = obj.CurrencyCode,
                CustomFormatting = obj.CustomFormatting,
                DisplayLocale = obj.DisplayLocale,
                DisplayOrder = obj.DisplayOrder,
                Id = obj.Id,
                LimitedToStores = obj.LimitedToStores,
                Name = obj.Name,
                Published = obj.Published,
                Rate = obj.Rate,
                UpdatedOnUtc = obj.UpdatedOnUtc
            };
        }

        public static CurrencyForCache Transform(Currency obj)
        {
            if (obj == null) return null;
            return new CurrencyForCache
            {
                CreatedOnUtc = obj.CreatedOnUtc,
                CurrencyCode = obj.CurrencyCode,
                CustomFormatting = obj.CustomFormatting,
                DisplayLocale = obj.DisplayLocale,
                DisplayOrder = obj.DisplayOrder,
                Id = obj.Id,
                LimitedToStores = obj.LimitedToStores,
                Name = obj.Name,
                Published = obj.Published,
                Rate = obj.Rate,
                UpdatedOnUtc = obj.UpdatedOnUtc
            };
        }
    }
}
