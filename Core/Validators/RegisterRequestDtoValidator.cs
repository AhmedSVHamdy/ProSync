using Core.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Validators
{
    public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("الاسم مطلوب.")
                .MinimumLength(3).WithMessage("الاسم يجب ألا يقل عن 3 أحرف.")
                .MaximumLength(100).WithMessage("الاسم يجب ألا يزيد عن 100 حرف.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.")
                .MaximumLength(200);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
                .MinimumLength(8).WithMessage("كلمة المرور يجب ألا تقل عن 8 أحرف.")
                .Matches("[A-Z]").WithMessage("يجب أن تحتوي على حرف كبير واحد على الأقل.")
                .Matches("[a-z]").WithMessage("يجب أن تحتوي على حرف صغير واحد على الأقل.")
                .Matches("[0-9]").WithMessage("يجب أن تحتوي على رقم واحد على الأقل.")
                .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي على رمز خاص واحد على الأقل.");

            RuleFor(x => x.TenantName)
                .NotEmpty().WithMessage("اسم الشركة مطلوب.")
                .MinimumLength(2).WithMessage("اسم الشركة يجب ألا يقل عن حرفين.")
                .MaximumLength(150);
        }
    }
}
