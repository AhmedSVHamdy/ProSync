using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.Authentication;
using Core.ServiceContracts;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ITokenIssuerService> _tokenIssuerServiceMock;   // ← بدل ITokenService
        private readonly Mock<IOtpService> _otpServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IGoogleAuthValidator> _googleAuthValidatorMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenIssuerServiceMock = new Mock<ITokenIssuerService>();
            _otpServiceMock = new Mock<IOtpService>();
            _emailServiceMock = new Mock<IEmailService>();
            _googleAuthValidatorMock = new Mock<IGoogleAuthValidator>();
            _configurationMock = new Mock<IConfiguration>();
            _mapperMock = new Mock<IMapper>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _passwordHasherMock.Object,
                _tokenIssuerServiceMock.Object,
                _otpServiceMock.Object,
                _emailServiceMock.Object,
                _googleAuthValidatorMock.Object,
                _configurationMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithNewEmail_CreatesUserAndSendsOtp()
        {
            var dto = new RegisterRequestDto
            {
                Name = "Ahmed",
                Email = "ahmed@test.com",
                Password = "P@ssw0rd123",
                TenantName = "Ahmed's Company"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            _passwordHasherMock.Setup(h => h.Hash(dto.Password)).Returns("hashed_password");
            _otpServiceMock.Setup(o => o.GenerateOtp()).Returns(("123456", "hashed_otp"));

            var result = await _authService.RegisterAsync(dto);

            result.Should().NotBeNull();
            result.Email.Should().Be(dto.Email);
            result.AccessToken.Should().BeNullOrEmpty();

            _userRepositoryMock.Verify(
                 r => r.AddTenantWithOwnerAsync(It.IsAny<Tenant>(), It.IsAny<User>(), It.IsAny<UserSettings>(), It.IsAny<Subscription>()),   // ← ضفنا It.IsAny<UserSettings>()
                 Times.Once);
            _emailServiceMock.Verify(e => e.SendOtpEmailAsync(dto.Email, "123456"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
        {
            var dto = new RegisterRequestDto
            {
                Name = "Ahmed",
                Email = "existing@test.com",
                Password = "P@ssw0rd123",
                TenantName = "Ahmed's Company"
            };

            var existingUser = new User { Id = Guid.NewGuid(), Email = dto.Email };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

            var act = async () => await _authService.RegisterAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("البريد الإلكتروني مستخدم بالفعل.");

            _userRepositoryMock.Verify(
                r => r.AddTenantWithOwnerAsync(It.IsAny<Tenant>(), It.IsAny<User>(), It.IsAny<UserSettings>(), It.IsAny<Subscription>()),   // ← ضفنا It.IsAny<UserSettings>()
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithCorrectCredentials_ReturnsAuthResponse()
        {
            var dto = new LoginRequestDto { Email = "ahmed@test.com", Password = "P@ssw0rd123" };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = "hashed_password",
                IsActive = true,
                IsEmailVerified = true
            };

            var expectedResponse = new AuthResponseDto
            {
                AccessToken = "fake_access_token",
                RefreshToken = "fake_refresh_token",
                Email = user.Email
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

            // Setup واحد بس دلوقتي، بدل الـ 4 اللي كانوا قبل الريفاكتور
            _tokenIssuerServiceMock.Setup(t => t.IssueTokensAsync(user)).ReturnsAsync(expectedResponse);

            var result = await _authService.LoginAsync(dto);

            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveAccount_ThrowsUnauthorizedAccessException()
        {
            var dto = new LoginRequestDto { Email = "ahmed@test.com", Password = "P@ssw0rd123" };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = "hashed_password",
                IsActive = false,   // ← الحساب معطّل
                IsEmailVerified = true
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

            var act = async () => await _authService.LoginAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("هذا الحساب غير نشط.");

            // نتأكد إن التوكنز ما بتتصدرش خالص لحساب معطّل
            _tokenIssuerServiceMock.Verify(t => t.IssueTokensAsync(It.IsAny<User>()), Times.Never);
        }
        [Fact]
        public async Task VerifyOtpAsync_WithCorrectCode_ActivatesAccount()
        {
            var dto = new VerifyOtpRequestDto { Email = "ahmed@test.com", OtpCode = "123456" };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                IsEmailVerified = false,
                OtpCodeHash = "hashed_otp",
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(3)
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _otpServiceMock.Setup(o => o.VerifyOtp(dto.OtpCode, user.OtpCodeHash, user.OtpExpiresAt)).Returns(true);

            await _authService.VerifyOtpAsync(dto);

            user.IsEmailVerified.Should().BeTrue();
            user.OtpCodeHash.Should().BeNull();
            user.OtpExpiresAt.Should().BeNull();

            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task VerifyOtpAsync_WithIncorrectCode_ThrowsInvalidOperationException()
        {
            var dto = new VerifyOtpRequestDto { Email = "ahmed@test.com", OtpCode = "000000" };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                IsEmailVerified = false,
                OtpCodeHash = "hashed_otp",
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(3)
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _otpServiceMock.Setup(o => o.VerifyOtp(dto.OtpCode, user.OtpCodeHash, user.OtpExpiresAt)).Returns(false);

            var act = async () => await _authService.VerifyOtpAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("كود التحقق غير صحيح أو منتهي الصلاحية.");

            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task VerifyOtpAsync_WithAlreadyVerifiedAccount_ThrowsInvalidOperationException()
        {
            var dto = new VerifyOtpRequestDto { Email = "ahmed@test.com", OtpCode = "123456" };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                IsEmailVerified = true   // مفعّل بالفعل
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

            var act = async () => await _authService.VerifyOtpAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("البريد الإلكتروني مُفعّل بالفعل.");

            // نتأكد إن VerifyOtp حتى ماتنادتش، لأن الفحص وقف قبلها
            _otpServiceMock.Verify(o => o.VerifyOtp(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
        }
        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokensAndRevokesOld()
        {
            var userId = Guid.NewGuid();
            var dto = new RefreshTokenRequestDto { AccessToken = "expired_token", RefreshToken = "raw_refresh_token" };

            var user = new User { Id = userId, Email = "ahmed@test.com" };

            var existingToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = "hashed_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsRevoked = false
            };

            var expectedResponse = new AuthResponseDto { AccessToken = "new_access_token", RefreshToken = "new_refresh_token" };

            _tokenIssuerServiceMock
                .Setup(t => t.ValidateAndGetUserIdFromExpiredTokenAsync(dto.AccessToken))
                .ReturnsAsync(userId);

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            _refreshTokenRepositoryMock
                .Setup(r => r.GetActiveTokensByUserIdAsync(userId))
                .ReturnsAsync(new List<RefreshToken> { existingToken });

            _passwordHasherMock
                .Setup(h => h.Verify(dto.RefreshToken, existingToken.TokenHash))
                .Returns(true);

            _tokenIssuerServiceMock.Setup(t => t.IssueTokensAsync(user)).ReturnsAsync(expectedResponse);

            var result = await _authService.RefreshTokenAsync(dto);

            result.Should().BeEquivalentTo(expectedResponse);
            existingToken.IsRevoked.Should().BeTrue();   // نتأكد إن القديم اتلغى (Rotation)

            _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(existingToken), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidAccessToken_ThrowsUnauthorizedAccessException()
        {
            var dto = new RefreshTokenRequestDto { AccessToken = "malformed_token", RefreshToken = "raw_refresh_token" };

            _tokenIssuerServiceMock
                .Setup(t => t.ValidateAndGetUserIdFromExpiredTokenAsync(dto.AccessToken))
                .ReturnsAsync((Guid?)null);   // التوكن مش قابل للتحقق

            var act = async () => await _authService.RefreshTokenAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("التوكن غير صالح.");
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredRefreshToken_ThrowsUnauthorizedAccessException()
        {
            var userId = Guid.NewGuid();
            var dto = new RefreshTokenRequestDto { AccessToken = "expired_token", RefreshToken = "raw_refresh_token" };

            var user = new User { Id = userId, Email = "ahmed@test.com" };

            var expiredToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = "hashed_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddDays(-1),   // ← منتهي بالفعل
                IsRevoked = false
            };

            _tokenIssuerServiceMock
                .Setup(t => t.ValidateAndGetUserIdFromExpiredTokenAsync(dto.AccessToken))
                .ReturnsAsync(userId);

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            _refreshTokenRepositoryMock
                .Setup(r => r.GetActiveTokensByUserIdAsync(userId))
                .ReturnsAsync(new List<RefreshToken> { expiredToken });

            _passwordHasherMock
                .Setup(h => h.Verify(dto.RefreshToken, expiredToken.TokenHash))
                .Returns(true);

            var act = async () => await _authService.RefreshTokenAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("جلسة الدخول منتهية، يرجى تسجيل الدخول مرة أخرى.");
        }
        [Fact]
        public async Task ForgotPasswordAsync_WithExistingEmail_SendsResetEmail()
        {
            var dto = new ForgotPasswordRequestDto { Email = "ahmed@test.com" };
            var user = new User { Id = Guid.NewGuid(), Email = dto.Email };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _otpServiceMock.Setup(o => o.GenerateOtp()).Returns(("654321", "hashed_otp"));

            await _authService.ForgotPasswordAsync(dto);

            _emailServiceMock.Verify(e => e.SendPasswordResetEmailAsync(dto.Email, "654321"), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNonExistingEmail_DoesNotThrowAndSendsNoEmail()
        {
            // فاكر مبدأ User Enumeration Prevention؟ ده بالظبط الاختبار اللي بيثبته
            var dto = new ForgotPasswordRequestDto { Email = "nonexistent@test.com" };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

            var act = async () => await _authService.ForgotPasswordAsync(dto);

            await act.Should().NotThrowAsync();   // لازم ماترميش Exception حتى لو الإيميل مش موجود
            _emailServiceMock.Verify(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithValidOtp_UpdatesPasswordAndRevokesTokens()
        {
            var dto = new ResetPasswordRequestDto { Email = "ahmed@test.com", OtpCode = "654321", NewPassword = "NewP@ss123" };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = "old_hashed_password",
                OtpCodeHash = "hashed_otp",
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(3)
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _otpServiceMock.Setup(o => o.VerifyOtp(dto.OtpCode, user.OtpCodeHash, user.OtpExpiresAt)).Returns(true);
            _passwordHasherMock.Setup(h => h.Hash(dto.NewPassword)).Returns("new_hashed_password");

            await _authService.ResetPasswordAsync(dto);

            user.PasswordHash.Should().Be("new_hashed_password");
            user.OtpCodeHash.Should().BeNull();

            _refreshTokenRepositoryMock.Verify(r => r.RevokeAllUserTokensAsync(user.Id), Times.Once);
        }
        [Fact]
        public async Task ChangePasswordAsync_WithCorrectCurrentPassword_UpdatesPassword()
        {
            var userId = Guid.NewGuid();
            var dto = new ChangePasswordRequestDto { CurrentPassword = "OldP@ss123", NewPassword = "NewP@ss123" };

            var user = new User { Id = userId, PasswordHash = "old_hashed_password" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(dto.CurrentPassword, user.PasswordHash)).Returns(true);
            _passwordHasherMock.Setup(h => h.Hash(dto.NewPassword)).Returns("new_hashed_password");

            await _authService.ChangePasswordAsync(userId, dto);

            user.PasswordHash.Should().Be("new_hashed_password");
            _refreshTokenRepositoryMock.Verify(r => r.RevokeAllUserTokensAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ThrowsUnauthorizedAccessException()
        {
            var userId = Guid.NewGuid();
            var dto = new ChangePasswordRequestDto { CurrentPassword = "WrongPassword", NewPassword = "NewP@ss123" };

            var user = new User { Id = userId, PasswordHash = "old_hashed_password" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(dto.CurrentPassword, user.PasswordHash)).Returns(false);

            var act = async () => await _authService.ChangePasswordAsync(userId, dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("كلمة المرور الحالية غير صحيحة.");

            _refreshTokenRepositoryMock.Verify(r => r.RevokeAllUserTokensAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ResendOtpAsync_WithUnverifiedAccount_SendsNewOtp()
        {
            var dto = new ResendOtpRequestDto { Email = "ahmed@test.com" };
            var user = new User { Id = Guid.NewGuid(), Email = dto.Email, IsEmailVerified = false };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
            _otpServiceMock.Setup(o => o.GenerateOtp()).Returns(("111222", "hashed_otp"));

            await _authService.ResendOtpAsync(dto);

            _emailServiceMock.Verify(e => e.SendOtpEmailAsync(dto.Email, "111222"), Times.Once);
        }

        [Fact]
        public async Task ResendOtpAsync_WithAlreadyVerifiedAccount_DoesNotSendOtp()
        {
            var dto = new ResendOtpRequestDto { Email = "ahmed@test.com" };
            var user = new User { Id = Guid.NewGuid(), Email = dto.Email, IsEmailVerified = true };   // مفعّل بالفعل

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);

            await _authService.ResendOtpAsync(dto);

            _emailServiceMock.Verify(e => e.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        [Fact]
        public async Task GetMeAsync_WithValidUserId_ReturnsMappedProfile()
        {
            var userId = Guid.NewGuid();
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Ahmed's Company" };

            var user = new User
            {
                Id = userId,
                Name = "Ahmed",
                Email = "ahmed@test.com",
                TenantId = tenant.Id,
                Tenant = tenant
            };

            var expectedDto = new UserProfileResponseDto
            {
                Id = userId,
                Name = "Ahmed",
                Email = "ahmed@test.com",
                TenantName = "Ahmed's Company"
            };

            _userRepositoryMock.Setup(r => r.GetByIdWithTenantAsync(userId)).ReturnsAsync(user);

            // فاكر ليه بنعمل Mock للـ Mapper نفسه؟ عشان إحنا بنختبر AuthService بس، مش AutoMapper نفسها
            _mapperMock.Setup(m => m.Map<UserProfileResponseDto>(user)).Returns(expectedDto);

            var result = await _authService.GetMeAsync(userId);

            result.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public async Task GetMeAsync_WithNonExistingUserId_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.GetByIdWithTenantAsync(userId)).ReturnsAsync((User?)null);

            var act = async () => await _authService.GetMeAsync(userId);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("المستخدم غير موجود.");
        }

        [Fact]
        public async Task DeleteAccountAsync_WithCorrectPassword_DeactivatesAccount()
        {
            var userId = Guid.NewGuid();
            var dto = new DeleteAccountRequestDto { Password = "P@ssw0rd123" };

            var user = new User
            {
                Id = userId,
                Email = "ahmed@test.com",
                Name = "Ahmed",
                PasswordHash = "hashed_password",
                IsActive = true
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

            await _authService.DeleteAccountAsync(userId, dto);

            user.IsActive.Should().BeFalse();
            user.Email.Should().Contain("deleted");
            user.Name.Should().Be("مستخدم محذوف");

            _refreshTokenRepositoryMock.Verify(r => r.RevokeAllUserTokensAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAccountAsync_WithIncorrectPassword_ThrowsUnauthorizedAccessException()
        {
            var userId = Guid.NewGuid();
            var dto = new DeleteAccountRequestDto { Password = "WrongPassword" };

            var user = new User { Id = userId, PasswordHash = "hashed_password" };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(false);

            var act = async () => await _authService.DeleteAccountAsync(userId, dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();

            user.IsActive.Should().BeTrue();   // نتأكد إن الحساب ماتأثرش خالص لما الباسورد غلط
        }
        [Fact]
        public async Task GoogleLoginAsync_WithNewUser_CreatesAccountAndReturnsTokens()
        {
            var dto = new GoogleLoginRequestDto { IdToken = "valid_google_token" };
            var payload = new GooglePayload { Email = "newgoogleuser@test.com", Name = "Google User" };

            var expectedResponse = new AuthResponseDto { AccessToken = "access_token", RefreshToken = "refresh_token" };

            _googleAuthValidatorMock.Setup(g => g.ValidateAsync(dto.IdToken)).ReturnsAsync(payload);
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(payload.Email)).ReturnsAsync((User?)null);

            _tokenIssuerServiceMock
                .Setup(t => t.IssueTokensAsync(It.Is<User>(u => u.Email == payload.Email)))
                .ReturnsAsync(expectedResponse);

            var result = await _authService.GoogleLoginAsync(dto);

            result.Should().BeEquivalentTo(expectedResponse);

            _userRepositoryMock.Verify(
                 r => r.AddTenantWithOwnerAsync(It.IsAny<Tenant>(), It.IsAny<User>(), It.IsAny<UserSettings>(), It.IsAny<Subscription>()),   // ← ضفنا It.IsAny<UserSettings>()
                 Times.Once);
        }

        [Fact]
        public async Task GoogleLoginAsync_WithExistingUser_DoesNotCreateNewAccount()
        {
            var dto = new GoogleLoginRequestDto { IdToken = "valid_google_token" };
            var payload = new GooglePayload { Email = "existinguser@test.com", Name = "Existing User" };

            var existingUser = new User { Id = Guid.NewGuid(), Email = payload.Email };
            var expectedResponse = new AuthResponseDto { AccessToken = "access_token", RefreshToken = "refresh_token" };

            _googleAuthValidatorMock.Setup(g => g.ValidateAsync(dto.IdToken)).ReturnsAsync(payload);
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(payload.Email)).ReturnsAsync(existingUser);
            _tokenIssuerServiceMock.Setup(t => t.IssueTokensAsync(existingUser)).ReturnsAsync(expectedResponse);

            var result = await _authService.GoogleLoginAsync(dto);

            result.Should().BeEquivalentTo(expectedResponse);

            _userRepositoryMock.Verify(
               r => r.AddTenantWithOwnerAsync(It.IsAny<Tenant>(), It.IsAny<User>(), It.IsAny<UserSettings>(), It.IsAny<Subscription>()),   // ← ضفنا It.IsAny<UserSettings>()
               Times.Once);
        }

        [Fact]
        public async Task GoogleLoginAsync_WithInvalidToken_ThrowsUnauthorizedAccessException()
        {
            var dto = new GoogleLoginRequestDto { IdToken = "invalid_token" };

            _googleAuthValidatorMock.Setup(g => g.ValidateAsync(dto.IdToken)).ReturnsAsync((GooglePayload?)null);

            var act = async () => await _authService.GoogleLoginAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("توكن جوجل غير صالح.");
        }
    }
}