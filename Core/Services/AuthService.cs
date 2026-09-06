using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.Authentication;
using Core.Enums;
using Core.ServiceContracts;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Hangfire;
using Microsoft.Extensions.Configuration;

namespace Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenIssuerService _tokenIssuerService;   // ← بدل ITokenService
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IGoogleAuthValidator _googleAuthValidator;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobService _backgroundJobService;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            ITokenIssuerService tokenIssuerService,   // ← بدل tokenService
            IOtpService otpService,
            IEmailService emailService,
            IGoogleAuthValidator googleAuthValidator,
            IConfiguration configuration,
            IMapper mapper,
            IBackgroundJobService backgroundJobService
            )
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _tokenIssuerService = tokenIssuerService;
            _otpService = otpService;
            _emailService = emailService;
            _googleAuthValidator = googleAuthValidator;
            _configuration = configuration;
            _mapper = mapper;
            _backgroundJobService = backgroundJobService;
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

            var userSettings = new UserSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                UserId = user.Id,
                EmailNotifications = true,
                NotificationsEnabled = true
            };

            // الإضافة الجديدة: Subscription افتراضية بالباقة المجانية
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                PlanTier = PlanTier.Free.ToString(),
                MaxEmployees = 5,           // فاكر حد الباقة المجانية اللي اتفقنا عليه بدري؟
                GitHubEnabled = false
            };

            await _userRepository.AddTenantWithOwnerAsync(tenant, user, userSettings, subscription);
            _backgroundJobService.Enqueue<IEmailService>(x => x.SendOtpEmailAsync(user.Email, rawOtp));

            return new AuthResponseDto
            {
                UserName = user.Name,
                Email = user.Email,
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

            if (!user.IsActive)
                throw new UnauthorizedAccessException("هذا الحساب غير نشط.");

            if (!user.IsEmailVerified)
                throw new InvalidOperationException("يجب تفعيل البريد الإلكتروني أولاً.");

            return await _tokenIssuerService.IssueTokensAsync(user);   // ← سطر واحد بدل الـ Method الكاملة
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var userId = await _tokenIssuerService.ValidateAndGetUserIdFromExpiredTokenAsync(dto.AccessToken)
                ?? throw new UnauthorizedAccessException("التوكن غير صالح.");

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("المستخدم غير موجود.");

            var storedTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

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

            matchedToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(matchedToken);

            return await _tokenIssuerService.IssueTokensAsync(user);
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
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user is null)
                return;

            var (rawOtp, otpHash) = _otpService.GenerateOtp();

            user.OtpCodeHash = otpHash;
            user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _userRepository.UpdateAsync(user);
            _backgroundJobService.Enqueue<IEmailService>(x => x.SendOtpEmailAsync(user.Email, rawOtp));
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

            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);
        }

        public async Task ResendOtpAsync(ResendOtpRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user is null || user.IsEmailVerified)
                return;

            var (rawOtp, otpHash) = _otpService.GenerateOtp();

            user.OtpCodeHash = otpHash;
            user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(5);

            await _userRepository.UpdateAsync(user);
            _backgroundJobService.Enqueue<IEmailService>(x => x.SendOtpEmailAsync(user.Email, rawOtp));
        }

        public async Task<UserProfileResponseDto> GetMeAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithTenantAsync(userId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            return _mapper.Map<UserProfileResponseDto>(user);   // ✅ AutoMapper بدل البناء اليدوي
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto dto)
        {
            var payload = await _googleAuthValidator.ValidateAsync(dto.IdToken)
                ?? throw new UnauthorizedAccessException("توكن جوجل غير صالح.");

            var user = await _userRepository.GetByEmailAsync(payload.Email);

            if (user is null)
            {
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
                    PasswordHash = string.Empty,
                    Role = UserRole.Owner.ToString(),
                    IsEmailVerified = true
                };

                var userSettings = new UserSettings
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    UserId = user.Id,
                    EmailNotifications = true,
                    NotificationsEnabled = true
                };

                var subscription = new Subscription
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    PlanTier = PlanTier.Free.ToString(),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1)
                };

                await _userRepository.AddTenantWithOwnerAsync(tenant, user, userSettings, subscription);
            }

            return await _tokenIssuerService.IssueTokensAsync(user);
        }

        public async Task DeleteAccountAsync(Guid userId, DeleteAccountRequestDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("كلمة المرور غير صحيحة.");

            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

            user.IsActive = false;
            user.Email = $"deleted_{user.Id}@deleted.prosync.com";
            user.Name = "مستخدم محذوف";

            await _userRepository.UpdateAsync(user);
        }
    }
}