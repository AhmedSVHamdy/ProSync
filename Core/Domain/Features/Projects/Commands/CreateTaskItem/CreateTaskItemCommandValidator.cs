using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.CreateTaskItem
{
    public class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
    {
        public CreateTaskItemCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان المهمة مطلوب.")
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000);

            RuleFor(x => x.Priority)
                .IsInEnum();

            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("معرّف المشروع مطلوب.");

            RuleFor(x => x.AssigneeId)
                .NotEmpty().WithMessage("يجب تعيين المهمة لموظف.");
        }
    }
}
