using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.Enums;
using Core.Helpers;
using Core.ServiceContracts;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static Core.Services.TokenService;

namespace Tests.Services
{
    public class InvitationServiceTests
    {
        private readonly Mock<IInvitationRepository> _invitationRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ITokenIssuerService> _tokenIssuerServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly InvitationService _invitationService;

        public InvitationServiceTests()
        {
            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _emailServiceMock = new Mock<IEmailService>();
            _tokenIssuerServiceMock = new Mock<ITokenIssuerService>();
            _configurationMock = new Mock<IConfiguration>();

            _invitationService = new InvitationService(
                _invitationRepositoryMock.Object,
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _emailServiceMock.Object,
                _tokenIssuerServiceMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task InviteUserAsync_WithNewEmail_CreatesInvitationAndSendsEmail()
        {
            var invitedByUserId = Guid.NewGuid();
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Ahmed's Company" };

            var invitingUser = new User
            {
                Id = invitedByUserId,
                TenantId = tenant.Id,
                Tenant = tenant,
                Role = UserRole.Owner.ToString()
            };

            var dto = new InviteUserRequestDto { Email = "newmember@test.com", Role = UserRole.Member };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(invitedByUserId)).ReturnsAsync(invitingUser);
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
            _configurationMock.Setup(c => c["ClientApp:BaseUrl"]).Returns("http://localhost:3000");

            await _invitationService.InviteUserAsync(invitedByUserId, dto);

            _invitationRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Invitation>(i =>
                    i.Email == dto.Email &&
                    i.Role == dto.Role &&
                    i.TenantId == tenant.Id &&
                    !i.IsAccepted)),
                Times.Once);

            _emailServiceMock.Verify(
                e => e.SendInvitationEmailAsync(dto.Email, It.IsAny<string>(), tenant.Name),
                Times.Once);
        }

        [Fact]
        public async Task InviteUserAsync_WithAlreadyRegisteredEmail_ThrowsInvalidOperationException()
        {
            var invitedByUserId = Guid.NewGuid();
            var invitingUser = new User { Id = invitedByUserId, TenantId = Guid.NewGuid() };
            var dto = new InviteUserRequestDto { Email = "existing@test.com", Role = UserRole.Member };

            var existingUser = new User { Id = Guid.NewGuid(), Email = dto.Email };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(invitedByUserId)).ReturnsAsync(invitingUser);
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

            var act = async () => await _invitationService.InviteUserAsync(invitedByUserId, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("هذا البريد الإلكتروني مسجل بالفعل.");

            _invitationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Invitation>()), Times.Never);
            _emailServiceMock.Verify(e => e.SendInvitationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AcceptInvitationAsync_WithValidToken_CreatesUserAndReturnsTokens()
        {
            var rawToken = "raw_invitation_token";
            var dto = new AcceptInvitationRequestDto
            {
                Token = rawToken,
                Name = "New Member",
                Password = "P@ssw0rd123"
            };

            var invitation = new Invitation
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Email = "newmember@test.com",
                Role = UserRole.Member,
                TokenHash = TokenHasher.HashDeterministic(rawToken),   // نفس منطق SHA256 اللي اتفقنا عليه
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsAccepted = false
            };

            var expectedResponse = new AuthResponseDto { AccessToken = "access_token", RefreshToken = "refresh_token" };

            _invitationRepositoryMock
                .Setup(r => r.GetPendingByTokenHashAsync(invitation.TokenHash))
                .ReturnsAsync(invitation);

            _passwordHasherMock.Setup(h => h.Hash(dto.Password)).Returns("hashed_password");

            _tokenIssuerServiceMock
                .Setup(t => t.IssueTokensAsync(It.Is<User>(u => u.Email == invitation.Email && u.Role == invitation.Role.ToString())))
                .ReturnsAsync(expectedResponse);

            var result = await _invitationService.AcceptInvitationAsync(dto);

            result.Should().BeEquivalentTo(expectedResponse);
            invitation.IsAccepted.Should().BeTrue();

            _userRepositoryMock.Verify(
                r => r.AddAsync(It.Is<User>(u => u.Email == invitation.Email && u.IsEmailVerified == true)),
                Times.Once);

            _invitationRepositoryMock.Verify(r => r.UpdateAsync(invitation), Times.Once);
        }

        [Fact]
        public async Task AcceptInvitationAsync_WithExpiredInvitation_ThrowsInvalidOperationException()
        {
            var dto = new AcceptInvitationRequestDto
            {
                Token = "expired_token",
                Name = "New Member",
                Password = "P@ssw0rd123"
            };

            _invitationRepositoryMock
                .Setup(r => r.GetPendingByTokenHashAsync(It.IsAny<string>()))
                .ReturnsAsync((Invitation?)null);   // فاكر إن GetPendingByTokenHashAsync أصلاً بترجع null لو منتهية أو متقبلة

            var act = async () => await _invitationService.AcceptInvitationAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("الدعوة غير صالحة أو منتهية الصلاحية.");

            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }
    }
}