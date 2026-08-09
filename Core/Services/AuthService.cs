using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.Enums;
using Core.ServiceContracts;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IGoogleAuthValidator _googleAuthValidator;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IOtpService otpService,
            IEmailService emailService,
            IConfiguration configuration,
            IGoogleAuthValidator googleAuthValidator)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _otpService = otpService;
            _emailService = emailService;
            _configuration = configuration;
            _googleAuthValidator = googleAuthValidator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser is not null)
                throw new InvalidOperationException("البريد الإلكتروني مستخدم بالفعل.");

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = dto.TenantName,
                PlanType = PlanTier.Free.ToString()
            };

            var (rawOtp, otpHash) = _otpService.GenerateOtp();

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Role = UserRole.Owner.ToString(),
                IsEmailVerified = false,
                OtpCodeHash = otpHash,
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            await _userRepository.AddTenantWithOwnerAsync(tenant, user);   // Method خاصة بيوضحها تحت
            await _emailService.SendOtpEmailAsync(user.Email, rawOtp);

            // مش بنرجع Access/Refresh Token هنا، لأن الحساب لسه مش متفعل (IsEmailVerified = false)
            return new AuthResponseDto
            {
                Email = user.Email,
                UserName = user.Name,
                Role = user.Role
            };
        }

        public async Task VerifyOtpAsync(VerifyOtpRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            if (user.IsEmailVerified)
                throw new InvalidOperationException("البريد الإلكتروني مُفعّل بالفعل.");

            var isValid = _otpService.VerifyOtp(dto.OtpCode, user.OtpCodeHash ?? string.Empty, user.OtpExpiresAt);
            if (!isValid)
                throw new InvalidOperationException("كود التحقق غير صحيح أو منتهي الصلاحية.");

            user.IsEmailVerified = true;
            user.OtpCodeHash = null;
            user.OtpExpiresAt = null;

            await _userRepository.UpdateAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedAccessException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");

            if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("البريد الإلكتروني أو كلمة المرور غير صحيحة.");

            if (!user.IsEmailVerified)
                throw new InvalidOperationException("يجب تفعيل البريد الإلكتروني أولاً.");

            return await GenerateAuthResponseAsync(user);
        }

        // Method خاصة مشتركة بين Login وRefreshToken وGoogleLogin وAcceptInvitation لاحقاً
        private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var (rawRefreshToken, refreshTokenHash) = _tokenService.GenerateRefreshToken();

            var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!);

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            var accessTokenMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                UserName = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var userId = await _tokenService.ValidateAndGetUserIdFromExpiredTokenAsync(dto.AccessToken)
                ?? throw new UnauthorizedAccessException("التوكن غير صالح.");

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("المستخدم غير موجود.");

            var storedTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);   // IEnumerable هنا، هوضحها تحت

            RefreshToken? matchedToken = null;
            foreach (var token in storedTokens)
            {
                if (_passwordHasher.Verify(dto.RefreshToken, token.TokenHash))
                {
                    matchedToken = token;
                    break;
                }
            }

            if (matchedToken is null || matchedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("جلسة الدخول منتهية، يرجى تسجيل الدخول مرة أخرى.");

            // Rotation: نلغي القديم ونولد واحد جديد، عشان لو حد سرق التوكن القديم يبقى عديم الفايدة فوراً
            matchedToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(matchedToken);

            return await GenerateAuthResponseAsync(user);
        }

        public async Task LogoutAsync(Guid userId, string refreshToken)
        {
            var storedTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

            foreach (var token in storedTokens)
            {
                if (_passwordHasher.Verify(refreshToken, token.TokenHash))
                {
                    token.IsRevoked = true;
                    await _refreshTokenRepository.UpdateAsync(token);
                    return;
                }
            }

            // مفيش Exception هنا لو التوكن مش موجود أصلاً — الهدف النهائي (اليوزر يبقى Logged out) محقق بأي حال
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user is null)
                return;   // مهم جداً: منرجعش Exception هنا (هنشرح ليه تحت)

            var (rawOtp, otpHash) = _otpService.GenerateOtp();

            user.OtpCodeHash = otpHash;
            user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _userRepository.UpdateAsync(user);
            await _emailService.SendPasswordResetEmailAsync(user.Email, rawOtp);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email)
                ?? throw new InvalidOperationException("طلب غير صالح.");

            var isValid = _otpService.VerifyOtp(dto.OtpCode, user.OtpCodeHash ?? string.Empty, user.OtpExpiresAt);
            if (!isValid)
                throw new InvalidOperationException("كود التحقق غير صحيح أو منتهي الصلاحية.");

            user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
            user.OtpCodeHash = null;
            user.OtpExpiresAt = null;

            await _userRepository.UpdateAsync(user);

            // إجراء أمني مهم: نلغي كل الـ Refresh Tokens بتاعة اليوزر، عشان أي جلسة قديمة (خصوصاً لو الباسورد اتسرق) تتقفل فوراً
            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("كلمة المرور الحالية غير صحيحة.");

            user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);   // نفس منطق ResetPassword
        }

        public async Task ResendOtpAsync(ResendOtpRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user is null || user.IsEmailVerified)
                return;   // نفس منطق User Enumeration Prevention، مفيش تفاصيل ترجع للمستخدم

            var (rawOtp, otpHash) = _otpService.GenerateOtp();

            user.OtpCodeHash = otpHash;
            user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _userRepository.UpdateAsync(user);
            await _emailService.SendOtpEmailAsync(user.Email, rawOtp);
        }

        public async Task<UserProfileResponseDto> GetMeAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithTenantAsync(userId)   // Method خاصة بترجع الـ User مع الـ Tenant، هوضحها تحت
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            return new UserProfileResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                TenantId = user.TenantId,
                TenantName = user.Tenant?.Name ?? string.Empty,
                IsEmailVerified = user.IsEmailVerified
            };
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto dto)
        {
            var payload = await _googleAuthValidator.ValidateAsync(dto.IdToken)   // Service خارجية جديدة، هنشرحها تحت
                ?? throw new UnauthorizedAccessException("توكن جوجل غير صالح.");

            var user = await _userRepository.GetByEmailAsync(payload.Email);

            if (user is null)
            {
                // أول مرة يدخل بجوجل، بننشئله Tenant و User جديدين تلقائياً (بنفس منطق Register)
                var tenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = $"{payload.Name}'s Workspace",
                    PlanType = PlanTier.Free.ToString()
                };

                user = new User
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Name = payload.Name,
                    Email = payload.Email,
                    PasswordHash = string.Empty,   // مفيش باسورد أصلاً، اليوزر داخل بجوجل بس
                    Role = UserRole.Owner.ToString(),
                    IsEmailVerified = true   // جوجل أصلاً أكد الإيميل، مش محتاجين OTP تاني
                };

                await _userRepository.AddTenantWithOwnerAsync(tenant, user);
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequestDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("كلمة المرور غير صحيحة.");

            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);
            await _userRepository.DeleteAsync(user);
        }

        
    }
}
