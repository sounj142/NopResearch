using System.Collections.Generic;
using Research.Core.Domain.Localization;
using Research.Core.Domain.Shipping;
using Research.Core.Domain.Stores;

namespace Research.Core.Domain.Directory
{
    /// <summary>
    /// Represents a country
    /// </summary>
    public partial class Country : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        private ICollection<StateProvince> _stateProvinces;

        /// <summary>
        /// Mối quan hệ ko hỗ trợ shipping method ở các quốc gia ... thường là quan hệ 1 chiều từ phía bảng ShippingMethod sang.
        /// Cần chú ý khi cài đặt bên bảng ShippingMethod, cài đặt cache static bảng này, đồng thời mỗi đối tượng của bảng sẽ duy
        /// trì 1 IList trỏ trực tiếp đến dữ liệu static cache của Country
        /// Bất kỳ sự thay đổi nào trên bảng mapping đều cần phải cập nhật lại IList của các ShippingMethod ( clear luôn các ShippingMethod
        /// để lấy data lại cho nó an toàn !)
        /// </summary>
        private ICollection<ShippingMethod> _restrictedShippingMethods;


        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether billing is allowed to this country
        /// </summary>
        public bool AllowsBilling { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shipping is allowed to this country
        /// </summary>
        public bool AllowsShipping { get; set; }

        /// <summary>
        /// Gets or sets the two letter ISO code
        /// </summary>
        public string TwoLetterIsoCode { get; set; }

        /// <summary>
        /// Gets or sets the three letter ISO code
        /// </summary>
        public string ThreeLetterIsoCode { get; set; }

        /// <summary>
        /// Gets or sets the numeric ISO code
        /// </summary>
        public int NumericIsoCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers in this country must be charged EU VAT
        /// </summary>
        public bool SubjectToVat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets the state/provinces
        /// </summary>
        public virtual ICollection<StateProvince> StateProvinces
        {
            get { return _stateProvinces ?? (_stateProvinces = new List<StateProvince>()); }
            protected set { _stateProvinces = value; }
        }

        /// <summary>
        /// Gets or sets the restricted shipping methods
        /// </summary>
        public virtual ICollection<ShippingMethod> RestrictedShippingMethods
        {
            get { return _restrictedShippingMethods ?? (_restrictedShippingMethods = new List<ShippingMethod>()); }
            protected set { _restrictedShippingMethods = value; }
        }

        public static Country MakeClone(Country country)
        {
            if (country != null) country = country.MakeClone();
            return country;
        }

        public Country MakeClone()
        {
            return new Country
            {
                AllowsBilling = this.AllowsBilling,
                AllowsShipping = this.AllowsShipping,
                DisplayOrder = this.DisplayOrder,
                Id = this.Id,
                LimitedToStores = this.LimitedToStores,
                Name = this.Name,
                NumericIsoCode = this.NumericIsoCode,
                Published = this.Published,
                SubjectToVat = this.SubjectToVat,
                ThreeLetterIsoCode = this.ThreeLetterIsoCode,
                TwoLetterIsoCode = this.TwoLetterIsoCode
            };
        }
    }
}
