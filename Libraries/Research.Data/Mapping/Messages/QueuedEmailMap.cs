﻿
using Research.Core.Domain.Messages;

namespace Research.Data.Mapping.Messages
{
    public partial class QueuedEmailMap : NopEntityTypeConfiguration<QueuedEmail>
    {
        public QueuedEmailMap()
        {
            this.ToTable("QueuedEmail");
            this.HasKey(qe => qe.Id);

            this.Property(qe => qe.From).IsRequired().HasMaxLength(500);
            this.Property(qe => qe.FromName).HasMaxLength(500);
            this.Property(qe => qe.To).IsRequired().HasMaxLength(500);
            this.Property(qe => qe.ToName).HasMaxLength(500);
            this.Property(qe => qe.ReplyTo).HasMaxLength(500);
            this.Property(qe => qe.ReplyToName).HasMaxLength(500);
            this.Property(qe => qe.CC).HasMaxLength(500);
            this.Property(qe => qe.Bcc).HasMaxLength(500);
            this.Property(qe => qe.Subject).HasMaxLength(1000);


            this.HasRequired(qe => qe.EmailAccount)
                .WithMany()
                .HasForeignKey(qe => qe.EmailAccountId).WillCascadeOnDelete(true);
        }
    }
}