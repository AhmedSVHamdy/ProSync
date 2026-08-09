using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Core.Enums;
using Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;

        public AdminUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserProfileResponseDto>> GetAllUsersInTenantAsync(Guid adminUserId)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            var users = await _userRepository.GetAllByTenantIdAsync(admin.TenantId);

            return users.Select(u => new UserProfileResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                TenantId = u.TenantId,
                IsEmailVerified = u.IsEmailVerified
            }).ToList();
        }

        public async Task DeactivateUserAsync(Guid adminUserId, Guid targetUserId)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            var targetUser = await GetTargetUserOrThrowAsync(admin.TenantId, targetUserId);

            if (targetUser.Id == admin.Id)
                throw new InvalidOperationException("لا يمكنك تعطيل حسابك الخاص.");

            if (targetUser.Role == UserRole.Owner.ToString())
                throw new InvalidOperationException("لا يمكن تعطيل حساب مالك الشركة.");

            targetUser.IsActive = false;
            await _userRepository.UpdateAsync(targetUser);
        }

        public async Task ActivateUserAsync(Guid adminUserId, Guid targetUserId)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            var targetUser = await GetTargetUserOrThrowAsync(admin.TenantId, targetUserId);

            targetUser.IsActive = true;
            await _userRepository.UpdateAsync(targetUser);
        }

        public async Task ChangeUserRoleAsync(Guid adminUserId, Guid targetUserId, UserRole newRole)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId)
                ?? throw new InvalidOperationException("المستخدم غير موجود.");

            var targetUser = await GetTargetUserOrThrowAsync(admin.TenantId, targetUserId);

            if (targetUser.Role == UserRole.Owner.ToString())
                throw new InvalidOperationException("لا يمكن تغيير دور مالك الشركة.");

            targetUser.Role = newRole.ToString();
            await _userRepository.UpdateAsync(targetUser);
        }

        // Method خاصة مشتركة بين الـ 3 Methods فوق، عشان منكررش نفس التحقق 3 مرات
        private async Task<User> GetTargetUserOrThrowAsync(Guid adminTenantId, Guid targetUserId)
        {
            var targetUser = await _userRepository.GetByIdAsync(targetUserId)
                ?? throw new InvalidOperationException("المستخدم المستهدف غير موجود.");

            if (targetUser.TenantId != adminTenantId)
                throw new UnauthorizedAccessException("لا يمكنك إدارة مستخدمين من شركة أخرى.");

            return targetUser;
        }
    }
}
