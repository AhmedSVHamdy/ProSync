using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Invitation : TenantEntity
    {
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string TokenHash { get; set; } = string.Empty;   // نفس منطق الـ OTP والـ RefreshToken، Hash مش القيمة الخام
        public DateTime ExpiresAt { get; set; }
        public bool IsAccepted { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public Guid InvitedByUserId { get; set; }
        public User InvitedBy { get; set; } = null!;
    }
}
