using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    namespace Core.Application.Contracts.Services
    {
        public interface IPasswordHasher
        {
            string Hash(string plainPassword);
            bool Verify(string plainPassword, string hashedPassword);
        }
    }
}
