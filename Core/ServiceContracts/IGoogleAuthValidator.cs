using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IGoogleAuthValidator
    {
        Task<GooglePayload?> ValidateAsync(string idToken);
    }

    public class GooglePayload
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
