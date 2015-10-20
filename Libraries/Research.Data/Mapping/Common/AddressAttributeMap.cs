using Research.Core.Domain.Common;

namespace Research.Data.Mapping.Common
{
    public partial class AddressAttributeMap : NopEntityTypeConfiguration<AddressAttribute>
    {
        public AddressAttributeMap()
        {
            this.ToTable("AddressAttribute");
            this.HasKey(aa => aa.Id);
            this.Property(aa => aa.Name).IsRequired().HasMaxLength(400);

            this.Ignore(aa => aa.AttributeControlType);
        }
    }
}
