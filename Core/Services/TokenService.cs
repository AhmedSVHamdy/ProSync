using Core.Domain.Entities;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private static readonly JsonWebTokenHandler _tokenHandler = new();

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new Dictionary<string, object>   // الـ API الجديد بياخد Dictionary مش List<Claim>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                ["TenantId"] = user.TenantId.ToString(),
                [ClaimTypes.Role] = user.Role.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var expirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Claims = claims,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = credentials
            };

            return _tokenHandler.CreateToken(tokenDescriptor);   // بترجع string مباشرة، من غير WriteToken منفصلة
        }

        public (string RawToken, string TokenHash) GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var rawToken = Convert.ToBase64String(randomBytes);
            var tokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken);

            return (rawToken, tokenHash);
        }

        public async Task<Guid?> ValidateAndGetUserIdFromExpiredTokenAsync(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!)),
                ValidateLifetime = false   // برضه false هنا، لأن التوكن أصلاً منتهي وده متوقع وقت الـ Refresh
            };

            var result = await _tokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters);

            if (!result.IsValid)
                return null;

            if (result.SecurityToken is not JsonWebToken jsonToken ||
                !jsonToken.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;   // نفس حماية الـ Algorithm Confusion Attack من قبل
            }

            var userIdClaim = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
