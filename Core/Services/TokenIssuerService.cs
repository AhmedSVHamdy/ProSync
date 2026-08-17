using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class TokenIssuerService : ITokenIssuerService
    {
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public TokenIssuerService(
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> IssueTokensAsync(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var (rawRefreshToken, refreshTokenHash) = _tokenService.GenerateRefreshToken();
            var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!);

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                IsRevoked = false
            });

            var accessTokenMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                UserName = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }
        public async Task<Guid?> ValidateAndGetUserIdFromExpiredTokenAsync(string accessToken)
        {
            return await _tokenService.ValidateAndGetUserIdFromExpiredTokenAsync(accessToken);
        }
    }
}
