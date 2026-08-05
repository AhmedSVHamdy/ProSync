using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class UserConfiguration : TenantEntityConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.ToTable("users");


            builder.HasIndex(u => new { u.TenantId, u.Email })
                .IsUnique();

            builder.Property(u => u.PasswordHash)
           .IsRequired();

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Name)
              .IsRequired()
              .HasMaxLength(150);
            builder.Property(u => u.Email)
              .IsRequired()
              .HasMaxLength(200);
           
            builder.Property(u => u.Role)
              .IsRequired()
              .HasMaxLength(50);

           
            builder.Property(u => u.RefreshToken)
                  .HasMaxLength(500)
                  .IsRequired(false);

            builder.Property(u => u.RefreshTokenExpirationDateTime)
                   .IsRequired(true);


            builder.HasOne(u => u.UserSettings)
                   .WithOne(u => u.User)
                   .HasForeignKey<UserSettings>(u => u.UserId)
                   .OnDelete(DeleteBehavior.Cascade); // لو اتحذف اليوزر تتحذف إعداداته

        }
    }
}
