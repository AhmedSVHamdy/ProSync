using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface IUserSettingsRepository : IGenericRepository<UserSettings>
    {
        Task<UserSettings?> GetByUserIdAsync(Guid userId);

    }
}
