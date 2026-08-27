using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO.Authentication
{
    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;   
        public string RefreshToken { get; set; } = string.Empty;
    }
}
