using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateTaskStatus
{
    public class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
    {
        public UpdateTaskStatusCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.NewStatus).IsInEnum();
        }
    }
}
