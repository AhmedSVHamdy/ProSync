using Core.DTO;
using Core.DTO.Authentication;
using Core.Validators;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Validators
{
    public class RegisterRequestDtoValidatorTests
    {
        private readonly RegisterRequestDtoValidator _validator;

        public RegisterRequestDtoValidatorTests()
        {
            _validator = new RegisterRequestDtoValidator();
        }

        [Fact]
        public void Validate_WithValidData_PassesValidation()
        {
            var dto = new RegisterRequestDto
            {
                Name = "Ahmed",
                Email = "ahmed@test.com",
                Password = "StrongP@ss123",
                TenantName = "ProSync Inc"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("short1!")]              // أقل من 8 أحرف
        [InlineData("alllowercase123!")]     // من غير حرف كبير
        [InlineData("ALLUPPERCASE123!")]     // من غير حرف صغير
        [InlineData("NoNumbersHere!")]       // من غير رقم
        [InlineData("NoSpecialChar123")]     // من غير رمز خاص
        public void Validate_WithWeakPassword_FailsValidation(string weakPassword)
        {
            var dto = new RegisterRequestDto
            {
                Name = "Ahmed",
                Email = "ahmed@test.com",
                Password = weakPassword,
                TenantName = "ProSync Inc"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-an-email")]
        [InlineData("@missingusername.com")]   // ← بدل "missing@domain"
        public void Validate_WithInvalidEmail_FailsValidation(string invalidEmail)
        {
            var dto = new RegisterRequestDto
            {
                Name = "Ahmed",
                Email = invalidEmail,
                Password = "StrongP@ss123",
                TenantName = "ProSync Inc"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_WithEmptyTenantName_FailsValidation()
        {
            var dto = new RegisterRequestDto
            {
                Name = "Ahmed",
                Email = "ahmed@test.com",
                Password = "StrongP@ss123",
                TenantName = ""
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TenantName);
        }
    }
}
