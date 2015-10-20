using Research.Core.Domain.Common;

namespace Research.Data.Mapping.Common
{
    public partial class SearchTermMap : NopEntityTypeConfiguration<SearchTerm>
    {
        public SearchTermMap()
        {
            this.ToTable("SearchTerm");
            this.HasKey(st => st.Id);
        }
    }
}
