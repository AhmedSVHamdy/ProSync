using Core.DTO;
using Core.Enums;
using Core.Validators;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Validators
{
    public class InviteUserRequestDtoValidatorTests
    {
        private readonly InviteUserRequestDtoValidator _validator;

        public InviteUserRequestDtoValidatorTests()
        {
            _validator = new InviteUserRequestDtoValidator();
        }

        [Fact]
        public void Validate_WithMemberRole_PassesValidation()
        {
            var dto = new InviteUserRequestDto { Email = "member@test.com", Role = UserRole.Member };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithOwnerRole_FailsValidation()
        {
            // فاكر ليه؟ الـ Owner مينفعش يتدّى بدعوة، بيتحدد بس وقت إنشاء الشركة
            var dto = new InviteUserRequestDto { Email = "someone@test.com", Role = UserRole.Owner };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Role)
                .WithErrorMessage("لا يمكن دعوة مستخدم بدور مالك الشركة.");
        }
    }
}
