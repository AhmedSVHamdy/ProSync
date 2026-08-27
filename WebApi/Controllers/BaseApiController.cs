using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("لم يتم التعرف على المستخدم.");

            return userId;
        }
        protected Guid GetCurrentTenantId()   // ← تأكد إن ده السطر موجود فعلاً
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
                throw new UnauthorizedAccessException("لم يتم التعرف على الشركة.");

            return tenantId;

        }
        protected string GetCurrentUserRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role))
                throw new UnauthorizedAccessException("لم يتم التعرف على دور المستخدم.");

            return role;
        }
    }
}
