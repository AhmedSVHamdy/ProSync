using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.Create_Sprint
{
    public class CreateSprintCommandValidator : AbstractValidator<CreateSprintCommand>
    {
        public CreateSprintCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان السبرنت مطلوب.")
                .MaximumLength(150);

            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("معرّف المشروع مطلوب.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("تاريخ البداية مطلوب.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("تاريخ النهاية مطلوب.")
                .GreaterThan(x => x.StartDate).WithMessage("تاريخ النهاية يجب أن يكون بعد تاريخ البداية.");
        }
    }
}
