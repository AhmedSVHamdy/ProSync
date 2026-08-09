using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class AcceptInvitationRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; internal set; }
    }
}
