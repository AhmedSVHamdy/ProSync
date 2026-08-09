using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.ServiceContracts;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public InvitationService(
            IInvitationRepository invitationRepository,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task InviteUserAsync(Guid invitedByUserId, InviteUserRequestDto dto)
        {
            var invitingUser = await _userRepository.GetByIdAsync(invitedByUserId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser is not null)
                throw new InvalidOperationException("هذا البريد الإلكتروني مسجل بالفعل.");

            var (rawToken, tokenHash) = _tokenService.GenerateRefreshToken();   // نفس آلية توليد التوكن العشوائي، مفيش داعي نكرر الكود

            var invitation = new Invitation
            {
                Id = Guid.NewGuid(),
                TenantId = invitingUser.TenantId,
                Email = dto.Email,
                Role = dto.Role,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsAccepted = false,
                CreatedAt = DateTime.UtcNow,
                InvitedByUserId = invitedByUserId
            };

            await _invitationRepository.AddAsync(invitation);

            var invitationLink = $"{_configuration["ClientApp:BaseUrl"]}/accept-invitation?token={rawToken}&email={dto.Email}";
            var tenantName = invitingUser.Tenant?.Name ?? string.Empty;

            await _emailService.SendInvitationEmailAsync(dto.Email, invitationLink, tenantName);
        }

        public async Task<AuthResponseDto> AcceptInvitationAsync(AcceptInvitationRequestDto dto)
        {
            var invitations = await _invitationRepository.GetPendingByEmailAsync(dto.Email);   // List من الدعوات المعلقة لنفس الإيميل (ممكن يكون مدعو أكتر من مرة)

            Invitation? matchedInvitation = null;
            foreach (var invitation in invitations)
            {
                if (_passwordHasher.Verify(dto.Token, invitation.TokenHash))
                {
                    matchedInvitation = invitation;
                    break;
                }
            }

            if (matchedInvitation is null || matchedInvitation.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("الدعوة غير صالحة أو منتهية الصلاحية.");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = matchedInvitation.TenantId,
                Name = dto.Name,
                Email = matchedInvitation.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Role = matchedInvitation.Role.ToString(),
                IsEmailVerified = true   // زي ما اتفقنا، مؤكد ضمنياً لأنه رد على دعوة بريدية
            };

            await _userRepository.AddAsync(newUser);   // دلوقتي AddAsync عادية كفاية، مفيش Tenant جديد هنا زي Register

            matchedInvitation.IsAccepted = true;
            await _invitationRepository.UpdateAsync(matchedInvitation);

            return await GenerateAuthResponseForNewUserAsync(newUser);   // Method مساعدة، هنشرحها تحت
        }

        // Method خاصة تكرر نفس منطق GenerateAuthResponseAsync في AuthService
        // (ملحوظة تصميمية مهمة أوضحها تحت)
        private async Task<AuthResponseDto> GenerateAuthResponseForNewUserAsync(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var (rawRefreshToken, refreshTokenHash) = _tokenService.GenerateRefreshToken();

            var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!);

            // هنا محتاجين IRefreshTokenRepository كمان في الكونستركتور، هنضيفها
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                UserName = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
