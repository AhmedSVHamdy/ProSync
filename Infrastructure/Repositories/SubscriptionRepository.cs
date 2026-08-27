using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(ProSyncContext context) : base(context) { }

        public async Task<Subscription?> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.TenantId == tenantId);
        }
    }
}
