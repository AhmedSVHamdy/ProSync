using Core.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Validators
{
    public class AcceptInvitationRequestDtoValidator : AbstractValidator<AcceptInvitationRequestDto>
    {
        public AcceptInvitationRequestDtoValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("رمز الدعوة مطلوب.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("الاسم مطلوب.")
                .MinimumLength(3).WithMessage("الاسم يجب ألا يقل عن 3 أحرف.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
                .MinimumLength(8).WithMessage("كلمة المرور يجب ألا تقل عن 8 أحرف.")
                .Matches("[A-Z]").WithMessage("يجب أن تحتوي على حرف كبير واحد على الأقل.")
                .Matches("[a-z]").WithMessage("يجب أن تحتوي على حرف صغير واحد على الأقل.")
                .Matches("[0-9]").WithMessage("يجب أن تحتوي على رقم واحد على الأقل.")
                .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي على رمز خاص واحد على الأقل.");
        }
    }
}
