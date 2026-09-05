using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class InviteUserRequestDto
    {
        public string Email { get; set; } = string.Empty;
        
        public UserRole Role { get; set; }
        public string? Specialty { get; set; }
    }
    public class InvitationResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsAccepted { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
