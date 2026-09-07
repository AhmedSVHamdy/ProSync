using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.ReassignTask
{
    public class ReassignTaskCommandValidator : AbstractValidator<ReassignTaskCommand>
    {
        public ReassignTaskCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.NewAssigneeId).NotEmpty();
        }
    }
}
