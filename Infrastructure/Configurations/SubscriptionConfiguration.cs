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
        public void Configure(EntityTypeBuilder<Subscription> builder)
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
            // كل Tenant له Subscription واحدة بس (One-to-One)
            builder.HasIndex(t => t.TenantId).IsUnique();

        }
    }
}
