﻿
using Research.Core.Domain.Blogs;

namespace Research.Data.Mapping.Blogs
{
    public partial class BlogCommentMap : NopEntityTypeConfiguration<BlogComment>
    {
        public BlogCommentMap()
        {
            this.ToTable("BlogComment");
            this.HasKey(pr => pr.Id);

            this.HasRequired(bc => bc.BlogPost)
                .WithMany(bp => bp.BlogComments)
                .HasForeignKey(bc => bc.BlogPostId);

            this.HasRequired(cc => cc.Customer)
                .WithMany()
                .HasForeignKey(cc => cc.CustomerId);
        }
    }
}
