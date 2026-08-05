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
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpirationDateTime { get; set; }

        public UserSettings? UserSettings { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
