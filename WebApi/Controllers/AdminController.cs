using Core.Domain.Entities;
using Core.Enums;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Owner,Admin")]
    public class AdminController : BaseApiController
    {
        private readonly IAdminUserService _adminUserService;

        public AdminController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        /// <summary>
        /// list all users in the tenant. Only users with the "Owner" or "Admin" role can access this endpoint.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var adminUserId = GetCurrentUserId();
            var result = await _adminUserService.GetAllUsersInTenantAsync(adminUserId);
            return Ok(result);
        }

        /// <summary>
        /// activate a user account. Only users with the "Owner" or "Admin" role can access this endpoint.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut("{userId}/deactivate")]
        public async Task<IActionResult> DeactivateUser(Guid userId)
        {
            var adminUserId = GetCurrentUserId();
            await _adminUserService.DeactivateUserAsync(adminUserId, userId);
            return Ok(new { message = "تم تعطيل الحساب بنجاح." });
        }

        /// <summary>
        /// activate a user account. Only users with the "Owner" or "Admin" role can access this endpoint.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut("{userId}/activate")]
        public async Task<IActionResult> ActivateUser(Guid userId)
        {
            var adminUserId = GetCurrentUserId();
            await _adminUserService.ActivateUserAsync(adminUserId, userId);
            return Ok(new { message = "تم تفعيل الحساب بنجاح." });
        }

        /// <summary>
        /// change a user's role. Only users with the "Owner" or "Admin" role can access this endpoint.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="newRole"></param>
        /// <returns></returns>
        [HttpPut("{userId}/role")]
        public async Task<IActionResult> ChangeUserRole(Guid userId, [FromQuery] UserRole newRole)
        {
            var adminUserId = GetCurrentUserId();
            await _adminUserService.ChangeUserRoleAsync(adminUserId, userId, newRole);
            return Ok(new { message = "تم تغيير دور المستخدم بنجاح." });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("لم يتم التعرف على المستخدم.");

            return userId;
        }
    }
}
