using Core.DTO;
using Core.Validators;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Validators
{
    public class ChangePasswordRequestDtoValidatorTests
    {
        private readonly ChangePasswordRequestDtoValidator _validator;

        public ChangePasswordRequestDtoValidatorTests()
        {
            _validator = new ChangePasswordRequestDtoValidator();
        }

        [Fact]
        public void Validate_WithValidData_PassesValidation()
        {
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldP@ss123",
                NewPassword = "NewP@ss456"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenNewPasswordSameAsCurrent_FailsValidation()
        {
            // ده أهم Test هنا — بيتأكد من قاعدة NotEqual اللي اتفقنا عليها بالتحديد
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "SameP@ss123",
                NewPassword = "SameP@ss123"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword)
                .WithErrorMessage("كلمة المرور الجديدة يجب أن تختلف عن الحالية.");
        }
    }
}
