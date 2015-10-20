using Research.Core.Domain.Customers;

namespace Research.Data.Mapping.Customers
{
    public partial class ExternalAuthenticationRecordMap : NopEntityTypeConfiguration<ExternalAuthenticationRecord>
    {
        public ExternalAuthenticationRecordMap()
        {
            this.ToTable("ExternalAuthenticationRecord");

            this.HasKey(ear => ear.Id);

            this.HasRequired(ear => ear.Customer)
                .WithMany(c => c.ExternalAuthenticationRecords)
                .HasForeignKey(ear => ear.CustomerId);

        }
    }
}
