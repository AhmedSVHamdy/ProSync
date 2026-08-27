using AutoMapper;
using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO.Authentication;
using Core.Enums;
using Core.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace UnitTest.Services
{
    public class AdminUserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AdminUserService _adminUserService;

        public AdminUserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _adminUserService = new AdminUserService(_userRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllUsersInTenantAsync_ReturnsMappedUsersInSameTenant()
        {
            var adminId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();

            var admin = new User { Id = adminId, TenantId = tenantId };
            var users = new List<User>
            {
                admin,
                new() { Id = Guid.NewGuid(), TenantId = tenantId }
            };

            var expectedDtos = new List<UserProfileResponseDto>
            {
                new() { Id = admin.Id },
                new() { Id = users[1].Id }
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
            _userRepositoryMock.Setup(r => r.GetAllByTenantIdAsync(tenantId)).ReturnsAsync(users);
            _mapperMock.Setup(m => m.Map<List<UserProfileResponseDto>>(users)).Returns(expectedDtos);

            var result = await _adminUserService.GetAllUsersInTenantAsync(adminId);

            result.Should().BeEquivalentTo(expectedDtos);
        }

        [Fact]
        public async Task DeactivateUserAsync_WithValidTarget_DeactivatesUser()
        {
            var tenantId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var admin = new User { Id = adminId, TenantId = tenantId, Role = UserRole.Owner.ToString() };
            var target = new User { Id = targetId, TenantId = tenantId, Role = UserRole.Member.ToString(), IsActive = true };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(targetId)).ReturnsAsync(target);

            await _adminUserService.DeactivateUserAsync(adminId, targetId);

            target.IsActive.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.UpdateAsync(target), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_WhenTargetIsOwner_ThrowsInvalidOperationException()
        {
            var tenantId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var admin = new User { Id = adminId, TenantId = tenantId, Role = UserRole.Admin.ToString() };
            var owner = new User { Id = ownerId, TenantId = tenantId, Role = UserRole.Owner.ToString() };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(ownerId)).ReturnsAsync(owner);

            var act = async () => await _adminUserService.DeactivateUserAsync(adminId, ownerId);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("لا يمكن تعطيل حساب مالك الشركة.");
        }

        [Fact]
        public async Task DeactivateUserAsync_WhenTargetIsSelf_ThrowsInvalidOperationException()
        {
            var adminId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var admin = new User { Id = adminId, TenantId = tenantId, Role = UserRole.Admin.ToString() };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);

            var act = async () => await _adminUserService.DeactivateUserAsync(adminId, adminId);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("لا يمكنك تعطيل حسابك الخاص.");
        }

        [Fact]
        public async Task DeactivateUserAsync_WhenTargetFromDifferentTenant_ThrowsUnauthorizedAccessException()
        {
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var admin = new User { Id = adminId, TenantId = Guid.NewGuid(), Role = UserRole.Admin.ToString() };
            var target = new User { Id = targetId, TenantId = Guid.NewGuid(), Role = UserRole.Member.ToString() };   // Tenant مختلف تماماً

            _userRepositoryMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(targetId)).ReturnsAsync(target);

            var act = async () => await _adminUserService.DeactivateUserAsync(adminId, targetId);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("لا يمكنك إدارة مستخدمين من شركة أخرى.");
        }

        [Fact]
        public async Task ActivateUserAsync_WithValidTarget_ActivatesUser()
        {
            var tenantId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var admin = new User { Id = adminId, TenantId = tenantId, Role = UserRole.Owner.ToString() };
            var target = new User { Id = targetId, TenantId = tenantId, Role = UserRole.Member.ToString(), IsActive = false };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(targetId)).ReturnsAsync(target);

            await _adminUserService.ActivateUserAsync(adminId, targetId);

            target.IsActive.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.UpdateAsync(target), Times.Once);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WithValidTarget_UpdatesRole()
        {
            var tenantId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var admin = new User { Id = adminId, TenantId = tenantId, Role = UserRole.Owner.ToString() };
            var target = new User { Id = targetId, TenantId = tenantId, Role = UserRole.Member.ToString() };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(targetId)).ReturnsAsync(target);

            await _adminUserService.ChangeUserRoleAsync(adminId, targetId, UserRole.Admin);

            target.Role.Should().Be(UserRole.Admin.ToString());
            _userRepositoryMock.Verify(r => r.UpdateAsync(target), Times.Once);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WhenTargetIsOwner_ThrowsInvalidOperationException()
        {
            var tenantId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var admin = new User { Id = adminId, TenantId = tenantId, Role = UserRole.Admin.ToString() };
            var owner = new User { Id = ownerId, TenantId = tenantId, Role = UserRole.Owner.ToString() };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(adminId)).ReturnsAsync(admin);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(ownerId)).ReturnsAsync(owner);

            var act = async () => await _adminUserService.ChangeUserRoleAsync(adminId, ownerId, UserRole.Member);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("لا يمكن تغيير دور مالك الشركة.");

            owner.Role.Should().Be(UserRole.Owner.ToString());   // نتأكد إن الدور فعلاً ما اتغيرش
        }
    }
}