using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.BreakdownProjectTasks
{
    public class BreakdownProjectTasksCommandValidator : AbstractValidator<BreakdownProjectTasksCommand>
    {
        public BreakdownProjectTasksCommandValidator()
        {
            RuleFor(x => x.ProjectId).NotEmpty();

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("وصف المهمة مطلوب.")
                .MinimumLength(20).WithMessage("الوصف قصير جداً، اكتب تفاصيل أكثر عشان الذكاء الاصطناعي يقدر يكسّره بشكل مفيد.")
                .MaximumLength(3000);

            RuleForEach(x => x.TeamMemberIds)
                .NotEmpty();
        }
    }
}
