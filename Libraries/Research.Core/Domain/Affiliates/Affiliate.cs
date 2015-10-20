using Research.Core.Domain.Common;

namespace Research.Core.Domain.Affiliates
{
    /// <summary>
    /// Miêu tả 1 chi nhánh ?
    /// </summary>
    public partial class Affiliate : BaseEntity
    {
        public int AddressId { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is active
        /// </summary>
        public bool Active { get; set; }

        public virtual Address Address { get; set; }
    }
}
