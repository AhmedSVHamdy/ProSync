using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IOtpService
    {
        (string RawCode, string CodeHash) GenerateOtp();
        bool VerifyOtp(string rawCode, string codeHash, DateTime? expiresAt);
    }
}
