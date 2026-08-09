using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class AuthResponseDto
    {
        public Guid? Id { get; set; }
        public string? PersonName { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
