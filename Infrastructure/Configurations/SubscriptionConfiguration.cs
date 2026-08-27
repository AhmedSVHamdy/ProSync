using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class SubscriptionConfiguration : TenantEntityConfiguration<Subscription>
    {
        public override void Configure(EntityTypeBuilder<Subscription> builder)
        {
            base.Configure(builder);

            builder.ToTable("Subscriptions");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.PlanTier)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(t => t.MaxEmployees)
                .IsRequired();
            builder.Property(t => t.PlanTier)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(t => t.StartDate)
                .IsRequired();
            builder.Property(t => t.EndDate)
                .IsRequired();
            // كل Tenant له Subscription واحدة بس (One-to-One)
            builder.HasIndex(t => t.TenantId).IsUnique();

        }
    }
}
