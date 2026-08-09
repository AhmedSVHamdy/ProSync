using Core.ServiceContracts.Core.Application.Contracts.Services;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;   // كل ما زاد الرقم، زاد الأمان لكن زاد وقت المعالجة. 12 هو المعيار المتوازن حالياً

        public string Hash(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
        }

        public bool Verify(string plainPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
    }
}
