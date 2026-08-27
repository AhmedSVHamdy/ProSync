using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class SprintRepository : GenericRepository<Sprint>, ISprintRepository
    {
        public SprintRepository(ProSyncContext context) : base(context) { }

        public async Task<List<Sprint>> GetByProjectIdAsync(Guid projectId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();
        }
    }
}
