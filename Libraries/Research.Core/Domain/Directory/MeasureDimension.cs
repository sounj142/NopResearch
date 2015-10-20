

namespace Research.Core.Domain.Directory
{
    /// <summary>
    /// Represents a measure dimension
    /// </summary>
    public partial class MeasureDimension : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the system keyword
        /// </summary>
        public string SystemKeyword { get; set; }

        /// <summary>
        /// Gets or sets the ratio
        /// </summary>
        public decimal Ratio { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        public MeasureDimension MakeClone()
        {
            return new MeasureDimension
            {
                DisplayOrder = this.DisplayOrder,
                Id = this.Id,
                Name = this.Name,
                Ratio = this.Ratio,
                SystemKeyword = this.SystemKeyword
            };
        }

        public static MeasureDimension MakeClone(MeasureDimension another)
        {
            if (another == null) return null;
            return another.MakeClone();
        }
    }
}
