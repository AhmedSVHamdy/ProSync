using Core.DTO;
using Core.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Validators
{
    public class InviteUserRequestDtoValidator : AbstractValidator<InviteUserRequestDto>
    {
        public InviteUserRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("الدور المحدد غير صالح.")
                .NotEqual(UserRole.Owner).WithMessage("لا يمكن دعوة مستخدم بدور مالك الشركة.");
        }
    }
}
