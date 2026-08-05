using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class UserSettings : TenantEntity
    {
        public Guid UserId { get; set; }
        public bool EmailNotifications { get; set; } = true;
        public bool NotificationsEnabled { get; set; } = true;

        public User User { get; set; } = null!;
    }
}
