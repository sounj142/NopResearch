﻿
using Research.Core.Domain.Forums;

namespace Research.Data.Mapping.Forums
{
    public partial class ForumPostMap : NopEntityTypeConfiguration<ForumPost>
    {
        public ForumPostMap()
        {
            this.ToTable("Forums_Post");
            this.HasKey(fp => fp.Id);
            this.Property(fp => fp.Text).IsRequired();
            this.Property(fp => fp.IPAddress).HasMaxLength(100);

            this.HasRequired(fp => fp.ForumTopic)
                .WithMany()
                .HasForeignKey(fp => fp.TopicId);

            this.HasRequired(fp => fp.Customer)
               .WithMany()
               .HasForeignKey(fp => fp.CustomerId)
               .WillCascadeOnDelete(false);
        }
    }
}
