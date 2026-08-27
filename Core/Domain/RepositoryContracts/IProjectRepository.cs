using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<List<Project>> GetAllByTenantIdAsync(Guid tenantId);
    }
}
