using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string TokenHash { get; set; } = string.Empty;   // برضه Hash مش القيمة الخام، لنفس سبب الـ OTP بالظبط
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
