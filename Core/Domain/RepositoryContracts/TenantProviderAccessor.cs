using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public static class TenantProviderAccessor
    {
        private static readonly AsyncLocal<Guid?> _tenantId = new();

        public static Guid? TenantId
        {
            get => _tenantId.Value;
            set => _tenantId.Value = value;
        }
    }
}
