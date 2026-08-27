using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface ISprintRepository : IGenericRepository<Sprint>
    {
        Task<List<Sprint>> GetByProjectIdAsync(Guid projectId);
    }
}
