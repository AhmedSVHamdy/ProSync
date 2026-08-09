using Core.Domain.Entities;
using Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface ITokenIssuerService
    {
        Task<AuthResponseDto> IssueTokensAsync(User user);
    }
}
