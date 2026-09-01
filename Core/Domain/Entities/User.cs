using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class User : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;
        public string? OtpCodeHash { get; set; }
        public DateTime? OtpExpiresAt { get; set; }

        public UserSettings? UserSettings { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public Tenant Tenant { get; set; } = null!;
    }
}
