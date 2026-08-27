using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateProject
{
    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Status).IsInEnum();
        }
    }
}
