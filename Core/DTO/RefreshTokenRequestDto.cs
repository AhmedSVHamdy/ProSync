using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;   
        public string RefreshToken { get; set; } = string.Empty;
    }
}
