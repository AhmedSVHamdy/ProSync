using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO.Authentication
{
    public class ResendOtpRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
