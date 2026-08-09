using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class UserProfileResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
    }
}
