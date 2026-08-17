using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ProSyncContext context) : base(context)
        {
        }

        public async Task<User?> GetByIdWithTenantAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.Tenant)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
                
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet

                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllByTenantIdAsync(Guid tenantId)
        {
            return await _dbSet
                .Include(u => u.Tenant)
                .AsNoTracking()
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task AddTenantWithOwnerAsync(Tenant tenant, User owner)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Tenants.AddAsync(tenant);
                    await _context.Users.AddAsync(owner);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
