using Core.DTO.Authentication;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Validators
{
    public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordRequestDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة.")
                .MinimumLength(8).WithMessage("كلمة المرور يجب ألا تقل عن 8 أحرف.")
                .Matches("[A-Z]").WithMessage("يجب أن تحتوي على حرف كبير واحد على الأقل.")
                .Matches("[a-z]").WithMessage("يجب أن تحتوي على حرف صغير واحد على الأقل.")
                .Matches("[0-9]").WithMessage("يجب أن تحتوي على رقم واحد على الأقل.")
                .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي على رمز خاص واحد على الأقل.")
                .NotEqual(x => x.CurrentPassword).WithMessage("كلمة المرور الجديدة يجب أن تختلف عن الحالية.");
        }
    }
}
