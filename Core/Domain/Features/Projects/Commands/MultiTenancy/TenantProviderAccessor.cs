using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.MultiTenancy
{
    public static class TenantProviderAccessor
    {
        private static readonly AsyncLocal<Guid?> _tenantId = new();
        private static readonly AsyncLocal<Guid?> _currentUserId = new();   // ← الإضافة الجديدة

        public static Guid? TenantId
        {
            get => _tenantId.Value;
            set => _tenantId.Value = value;
        }

        public static Guid? CurrentUserId   // ← الإضافة الجديدة
        {
            get => _currentUserId.Value;
            set => _currentUserId.Value = value;
        }
    }
}
