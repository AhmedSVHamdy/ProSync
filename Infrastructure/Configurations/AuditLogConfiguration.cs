using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class AuditLogConfiguration : TenantEntityConfiguration<AuditLog>
    {
        public override void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            base.Configure(builder);

            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.Id);
                 
            builder.Property(a => a.Action)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            builder.Property(a => a.Timestamp)
                .IsRequired();
            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(a => a.TaskItem)
                 .WithMany()
                 .HasForeignKey(a => a.TaskItemId)
                 .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
