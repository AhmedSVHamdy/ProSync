using Core.DTO;
using Core.ServiceContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetSubscriptions
{
    public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionResponseDto>
    {
        private readonly ISubscriptionService _subscriptionService;

        public GetSubscriptionQueryHandler(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public async Task<SubscriptionResponseDto> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
        {
            var cached = await _subscriptionService.GetSubscriptionAsync(request.TenantId);

            return new SubscriptionResponseDto
            {
                PlanTier = cached.PlanTier,
                MaxEmployees = cached.MaxEmployees,
                GitHubEnabled = cached.GitHubEnabled
            };
        }
    }
}
