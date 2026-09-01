using Core.Domain.Features.Projects.Commands.MultiTenancy;
using Core.Domain.RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class TenantProviderRepositories : ITenantProviderRepository
    {
        public Guid? TenantId => TenantProviderAccessor.TenantId;
    }
}
