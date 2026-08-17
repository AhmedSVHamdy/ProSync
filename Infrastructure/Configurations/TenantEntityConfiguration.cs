using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public abstract class TenantEntityConfiguration<T> : IEntityTypeConfiguration<T>
       where T : TenantEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasQueryFilter(x => x.TenantId == TenantProviderAccessor.TenantId);
            ConfigureTenantRelation(builder);
        }

        // Virtual — الإعداد الافتراضي للكيانات اللي مالهاش Navigation Property لـ Tenant
        protected virtual void ConfigureTenantRelation(EntityTypeBuilder<T> builder)
        {
            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

