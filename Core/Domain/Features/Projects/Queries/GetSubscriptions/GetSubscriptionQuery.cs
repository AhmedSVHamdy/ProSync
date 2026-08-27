using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetSubscriptions
{
    public class GetSubscriptionQuery : IRequest<SubscriptionResponseDto>
    {
        public Guid TenantId { get; set; }
    }
}
