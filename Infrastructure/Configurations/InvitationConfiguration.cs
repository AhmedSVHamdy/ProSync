using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class InvitationConfiguration : TenantEntityConfiguration<Invitation>
    {
        public override void Configure(EntityTypeBuilder<Invitation> builder)
        {
            base.Configure(builder);   // Tenant relation + Query Filter تلقائي

            builder.ToTable("Invitations");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.TokenHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(i => i.ExpiresAt).IsRequired();
            builder.Property(i => i.CreatedAt).IsRequired();
            builder.Property(i => i.IsAccepted).HasDefaultValue(false);

            builder.HasOne(i => i.InvitedBy)
                .WithMany()
                .HasForeignKey(i => i.InvitedByUserId)
                .OnDelete(DeleteBehavior.NoAction);   // نفس منطق AuditLog، تفادي Cascade Cycle مع Tenant

            builder.HasIndex(i => i.TokenHash);
        }
    }
}
