using Core.Domain.Entities;
using Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
        Task RevokeAllUserTokensAsync(Guid userId);
    }
}
