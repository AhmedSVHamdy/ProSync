using Core.DTO.Authentication;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Validators
{
    public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestDtoValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("الـ Access Token مطلوب.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("الـ Refresh Token مطلوب.");
        }
    }
}
