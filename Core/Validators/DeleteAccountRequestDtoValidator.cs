using Core.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Validators
{
    public class DeleteAccountRequestDtoValidator : AbstractValidator<DeleteAccountRequestDto>
    {
        public DeleteAccountRequestDtoValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة لتأكيد حذف الحساب.");
        }
    }
}
