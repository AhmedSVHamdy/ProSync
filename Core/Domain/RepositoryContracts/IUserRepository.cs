using Core.Domain.Entities;
using Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByIdWithTenantAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllByTenantIdAsync(Guid tenantId);
        Task AddTenantWithOwnerAsync(Tenant tenant, User owner);
    }
}
