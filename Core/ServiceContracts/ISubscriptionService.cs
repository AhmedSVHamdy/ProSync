using Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface ISubscriptionService
    {
        Task<SubscriptionCacheDto> GetSubscriptionAsync(Guid tenantId);
        Task InvalidateCacheAsync(Guid tenantId);
    }
}
