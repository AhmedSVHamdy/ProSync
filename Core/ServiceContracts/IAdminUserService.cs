using Core.DTO.Authentication;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IAdminUserService
    {
        Task<List<UserProfileResponseDto>> GetAllUsersInTenantAsync(Guid adminUserId);
        Task DeactivateUserAsync(Guid adminUserId, Guid targetUserId);
        Task ActivateUserAsync(Guid adminUserId, Guid targetUserId);
        Task ChangeUserRoleAsync(Guid adminUserId, Guid targetUserId, UserRole newRole);
    }
}
