using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class UserSettingsConfiguration : TenantEntityConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            base.Configure(builder);
            builder.ToTable("UserSettings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EmailNotifications)
                   .HasDefaultValue(true); // افتراضياً تفعيل الإشعارات

            builder.Property(x => x.NotificationsEnabled)
                   .HasDefaultValue(true);

            // العلاقات
            // تم تعريف العلاقة في UserConfiguration، ولكن للتأكيد من الطرفين:
            builder.HasOne<User>()
                   .WithOne(x => x.UserSettings)
                   .HasForeignKey<UserSettings>(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
