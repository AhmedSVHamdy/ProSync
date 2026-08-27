using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ICacheService _cacheService;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository, ICacheService cacheService)
        {
            _subscriptionRepository = subscriptionRepository;
            _cacheService = cacheService;
        }

        public async Task<SubscriptionCacheDto> GetSubscriptionAsync(Guid tenantId)
        {
            var cacheKey = $"subscription:{tenantId}";

            // الخطوة 1: هل موجودة في الكاش؟ (فاكر "المكتب في الغرفة"؟)
            var cached = await _cacheService.GetAsync<SubscriptionCacheDto>(cacheKey);
            if (cached is not null)
            {
                return cached;   // لقيناها في الكاش، رجّعها فوراً، من غير ما نروح للداتابيز خالص
            }

            // الخطوة 2: مش موجودة؟ روح جيبها من الداتابيز (فاكر "المكتبة البعيدة"؟)
            var subscription = await _subscriptionRepository.GetByTenantIdAsync(tenantId)
                ?? throw new InvalidOperationException("الاشتراك غير موجود.");

            var dto = new SubscriptionCacheDto
            {
                PlanTier = subscription.PlanTier,
                MaxEmployees = subscription.MaxEmployees,
                GitHubEnabled = subscription.GitHubEnabled
            };

            // الخطوة 3: خزّنها في الكاش عشان المرة الجاية (فاكر "حط نسخة على مكتبك"؟)
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromHours(1));

            return dto;
        }

        public async Task InvalidateCacheAsync(Guid tenantId)
        {
            var cacheKey = $"subscription:{tenantId}";
            await _cacheService.RemoveAsync(cacheKey);
        }
    }
}
