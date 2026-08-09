using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class ResetPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;       // نفس آلية الـ OTP اللي هنستخدمها في تأكيد الإيميل
        public string NewPassword { get; set; } = string.Empty;
    }
}
