
using Research.Core.Domain.Forums;

namespace Research.Data.Mapping.Forums
{
    public partial class ForumGroupMap : NopEntityTypeConfiguration<ForumGroup>
    {
        public ForumGroupMap()
        {
            this.ToTable("Forums_Group");
            this.HasKey(fg => fg.Id);
            this.Property(fg => fg.Name).IsRequired().HasMaxLength(200);
        }
    }
}

