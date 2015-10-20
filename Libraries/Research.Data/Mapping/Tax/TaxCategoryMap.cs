
using Research.Core.Domain.Tax;

namespace Research.Data.Mapping.Tax
{
    public class TaxCategoryMap : NopEntityTypeConfiguration<TaxCategory>
    {
        public TaxCategoryMap()
        {
            this.ToTable("TaxCategory");
            this.HasKey(tc => tc.Id);
            this.Property(tc => tc.Name).IsRequired().HasMaxLength(400);
        }
    }
}
