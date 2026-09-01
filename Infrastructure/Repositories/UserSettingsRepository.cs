using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class UserSettingsRepository : GenericRepository<UserSettings>, IUserSettingsRepository
    {
        public UserSettingsRepository(ProSyncContext context) : base(context) { }

        public async Task<UserSettings?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
        .IgnoreQueryFilters()   // ← تأكد إنها موجودة
        .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
