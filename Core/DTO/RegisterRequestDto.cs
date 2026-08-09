using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class RegisterRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; }= string.Empty;
        public string TenantName { get; set; } = string.Empty;   // اسم الشركة وقت إنشاء الـ Workspace لأول مرة
    }
}
