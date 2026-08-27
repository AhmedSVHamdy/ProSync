using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(ProSyncContext context) : base(context) { }

        public async Task<List<Project>> GetAllByTenantIdAsync(Guid tenantId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.TenantId == tenantId)
                .ToListAsync();
        }
    }
}
