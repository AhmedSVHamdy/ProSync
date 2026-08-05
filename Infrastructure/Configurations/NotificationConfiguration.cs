using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class NotificationConfiguration : TenantEntityConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            base.Configure(builder);

            builder.ToTable("Notifications");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Message)
                   .IsRequired()
                   .HasMaxLength(500); // محتوى الإشعار

            builder.Property(x => x.Type)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.ReferenceId)
                   .IsRequired(false);

            // Payload: بيانات إضافية (JSON عادةً)، يمكن أن تكون Null
            builder.Property(x => x.Payload)
                   .IsRequired(false);

            builder.Property(x => x.IsRead)
                   .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            // Relationships

            // العلاقة مع  User
            builder.HasOne(x => x.User)
                   .WithMany(x => x.Notifications) // ✅ التعديل هنا: ربطناها بالليستة اللي في اليوزر عشان ميعملش UserId1
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
