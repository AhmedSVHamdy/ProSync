using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public  void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            
            builder.ToTable("RefreshTokens");
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.TokenHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rt => rt.ExpiresAt).IsRequired();
            builder.Property(rt => rt.CreatedAt).IsRequired();
            builder.Property(rt => rt.IsRevoked).HasDefaultValue(false);

            builder.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);   // لو اليوزر اتمسح، الـ Refresh Tokens بتاعته تتمسح معاه (منطقي هنا، عكس AuditLog)

            // فهرس يسرع البحث وقت الـ Refresh (هنقارن بالـ Hash كتير)
            builder.HasIndex(rt => rt.TokenHash);
        }
    }
}
