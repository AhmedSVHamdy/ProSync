using Core.Domain.Entities;
using Core.DTO.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface ITokenIssuerService
    {
        Task<AuthResponseDto> IssueTokensAsync(User user);
        Task<Guid?> ValidateAndGetUserIdFromExpiredTokenAsync(string accessToken);
    }
}
