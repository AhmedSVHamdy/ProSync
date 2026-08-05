using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface ITenantProviderRepository
    {
        Guid? TenantId { get; }
    }
}
