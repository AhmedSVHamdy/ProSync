using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Helpers
{
    public static class TokenHasher
    {
        public static string HashDeterministic(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
