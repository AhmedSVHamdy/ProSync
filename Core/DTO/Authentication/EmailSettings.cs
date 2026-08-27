using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO.Authentication
{
    public class EmailSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // App Password مش الباسورد العادي
        public string Host { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
