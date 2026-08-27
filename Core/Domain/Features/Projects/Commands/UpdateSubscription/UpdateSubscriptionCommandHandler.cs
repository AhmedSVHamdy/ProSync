using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateSubscription
{
    public class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, SubscriptionResponseDto>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly ISubscriptionService _subscriptionService;   // ← عشان نمسح الكاش القديم

        public UpdateSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            ISubscriptionService subscriptionService)
        {
            _subscriptionRepository = subscriptionRepository;
            _subscriptionService = subscriptionService;
        }

        public async Task<SubscriptionResponseDto> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId)
                ?? throw new InvalidOperationException("الاشتراك غير موجود.");

            // فاكر الجدول اللي عملناه زمان في خطة المشروع (Free/Pro/Enterprise)؟
            subscription.PlanTier = request.NewPlanTier.ToString();
            (subscription.MaxEmployees, subscription.GitHubEnabled) = request.NewPlanTier switch
            {
                Core.Enums.PlanTier.Free => (5, false),
                Core.Enums.PlanTier.Pro => (25, true),
                Core.Enums.PlanTier.Enterprise => (int.MaxValue, true),
                _ => throw new InvalidOperationException("باقة غير معروفة.")
            };

            await _subscriptionRepository.UpdateAsync(subscription);

            // نقطة مهمة جداً: فاكر ليه عملنا InvalidateCacheAsync أصلاً؟ ده استخدامها الحقيقي الأول
            await _subscriptionService.InvalidateCacheAsync(request.TenantId);

            return new SubscriptionResponseDto
            {
                Id = subscription.Id,
                PlanTier = subscription.PlanTier,
                MaxEmployees = subscription.MaxEmployees,
                GitHubEnabled = subscription.GitHubEnabled
            };
        }
    }
}
