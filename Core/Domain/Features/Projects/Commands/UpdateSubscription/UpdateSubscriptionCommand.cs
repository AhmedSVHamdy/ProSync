using Core.DTO;
using Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.UpdateSubscription
{
    public class UpdateSubscriptionCommand : IRequest<SubscriptionResponseDto>
    {
        public PlanTier NewPlanTier { get; set; }

        [JsonIgnore]
        public Guid TenantId { get; set; }   // فاكر القاعدة الثابتة اللي اتفقنا عليها؟ من الـ JWT، مش من اليوزر
    }
}
