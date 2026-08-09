using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    namespace Core.Application.Contracts.Services
    {
        public interface ITokenService
        {
            string GenerateAccessToken(User user);
            (string RawToken, string TokenHash) GenerateRefreshToken();
            Task<Guid?> ValidateAndGetUserIdFromExpiredTokenAsync(string accessToken);
        }
    }
}
