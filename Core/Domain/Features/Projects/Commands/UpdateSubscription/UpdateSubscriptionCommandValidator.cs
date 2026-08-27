using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateSubscription
{
    public class UpdateSubscriptionCommandValidator : AbstractValidator<UpdateSubscriptionCommand>
    {
        public UpdateSubscriptionCommandValidator()
        {
            RuleFor(x => x.NewPlanTier).IsInEnum();
        }
    }
}
