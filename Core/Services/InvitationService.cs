using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.DTO.Authentication;
using Core.Helpers;
using Core.ServiceContracts;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using static Core.Services.TokenService;

namespace Core.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly ITokenIssuerService _tokenIssuerService;
        private readonly IConfiguration _configuration;
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly ISubscriptionService _subscriptionService;

        public InvitationService(
            IInvitationRepository invitationRepository,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            ITokenIssuerService tokenIssuerService,
            IConfiguration configuration,
            IUserSettingsRepository userSettingsRepository,
            ISubscriptionService subscriptionService)
        {
            _invitationRepository = invitationRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _tokenIssuerService = tokenIssuerService;
            _configuration = configuration;
            _userSettingsRepository = userSettingsRepository;
            _subscriptionService = subscriptionService;
        }

        public async Task InviteUserAsync(Guid invitedByUserId, InviteUserRequestDto dto)
        {
            var invitingUser = await _userRepository.GetByIdAsync(invitedByUserId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser is not null)
                throw new InvalidOperationException("هذا البريد الإلكتروني مسجل بالفعل.");

            var subscription = await _subscriptionService.GetSubscriptionAsync(invitingUser.TenantId);
            var currentEmployeeCount = await _userRepository.GetEmployeeCountByTenantIdAsync(invitingUser.TenantId);

            if (currentEmployeeCount >= subscription.MaxEmployees)
            {
                throw new InvalidOperationException(
                    $"وصلت لحد الموظفين المسموح به في باقتك الحالية ({subscription.MaxEmployees}). يرجى الترقية لدعوة المزيد.");
            }

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenHash = TokenHasher.HashDeterministic(rawToken);

            var invitation = new Invitation
            {
                Id = Guid.NewGuid(),
                TenantId = invitingUser.TenantId,
                Email = dto.Email,
                Role = dto.Role,
                Specialty = dto.Specialty,
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
            var tokenHash = TokenHasher.HashDeterministic(dto.Token);

            var invitation = await _invitationRepository.GetPendingByTokenHashAsync(tokenHash)
                ?? throw new InvalidOperationException("الدعوة غير صالحة أو منتهية الصلاحية.");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = invitation.TenantId,
                Name = dto.Name,
                Email = invitation.Email,
                PasswordHash = _passwordHasher.Hash(dto.Password),
                Role = invitation.Role.ToString(),
                Specialty = invitation.Specialty,
                IsEmailVerified = true
            };

            await _userRepository.AddAsync(newUser);

            var userSettings = new UserSettings
            {
                Id = Guid.NewGuid(),
                TenantId = invitation.TenantId,
                UserId = newUser.Id,
                EmailNotifications = true,
                NotificationsEnabled = true
            };

            await _userSettingsRepository.AddAsync(userSettings);

            invitation.IsAccepted = true;
            await _invitationRepository.UpdateAsync(invitation);

            return await _tokenIssuerService.IssueTokensAsync(newUser);
        }
    }
}