using Core.ServiceContracts;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Core.Services
{
    public class OtpService : IOtpService
    {
        public (string RawCode, string CodeHash) GenerateOtp()
        {
            var rawCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var codeHash = BCrypt.Net.BCrypt.HashPassword(rawCode);

            return (rawCode, codeHash);
        }

        public bool VerifyOtp(string rawCode, string codeHash, DateTime? expiresAt)
        {
            if (expiresAt is null || expiresAt < DateTime.UtcNow)
                return false;   // منتهي الصلاحية، ولا داعي حتى نتحقق من الكود نفسه

            return BCrypt.Net.BCrypt.Verify(rawCode, codeHash);
        }
    }
}
