using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.CreateProject
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم المشروع مطلوب.")
                .MaximumLength(150);
        }
    }
}
