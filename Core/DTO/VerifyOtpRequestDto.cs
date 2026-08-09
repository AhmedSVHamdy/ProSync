using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class VerifyOtpRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
}
