﻿using Research.Core.Domain.Logging;

namespace Research.Data.Mapping.Logging
{
    public partial class LogMap : NopEntityTypeConfiguration<Log>
    {
        public LogMap()
        {
            this.ToTable("Log");
            this.HasKey(l => l.Id);
            this.Property(l => l.ShortMessage).IsRequired();
            this.Property(l => l.IpAddress).HasMaxLength(200);

            this.Ignore(l => l.LogLevel);

            this.HasOptional(l => l.Customer)
                .WithMany()
                .HasForeignKey(l => l.CustomerId)
            .WillCascadeOnDelete(true);

        }
    }
}
