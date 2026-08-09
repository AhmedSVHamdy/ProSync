using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class InvitationRepository : GenericRepository<Invitation>, IInvitationRepository
    {
        public InvitationRepository(ProSyncContext context) : base(context) { }

        public async Task<List<Invitation>> GetPendingByEmailAsync(string email)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(i => i.Email == email && !i.IsAccepted && i.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
    }
}
