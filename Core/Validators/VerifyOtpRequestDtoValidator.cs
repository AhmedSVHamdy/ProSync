using Core.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Validators
{
    public class VerifyOtpRequestDtoValidator : AbstractValidator<VerifyOtpRequestDto>
    {
        public VerifyOtpRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("كود التحقق مطلوب.")
                .Length(6).WithMessage("كود التحقق يجب أن يكون 6 أرقام.")
                .Matches("^[0-9]+$").WithMessage("كود التحقق يجب أن يتكون من أرقام فقط.");
        }
    }
}
