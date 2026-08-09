using Core.Domain.Entities;
using Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface IInvitationRepository : IGenericRepository<Invitation>
    {
        Task<List<Invitation>> GetPendingByEmailAsync(string email);
    }
}
