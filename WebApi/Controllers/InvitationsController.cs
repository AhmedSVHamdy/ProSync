using Core.Domain.Entities;
using Core.DTO;
using Core.DTO.Authentication;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/invitations")]
    public class InvitationsController : BaseApiController
    {
        private readonly IInvitationService _invitationService;

        public InvitationsController(IInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        /// <summary>
        /// sends an invitation to a user to join the system. Only users with the "Owner" or "Admin" role can send invitations.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserRequestDto dto)
        {
            var invitedByUserId = GetCurrentUserId();
            await _invitationService.InviteUserAsync(invitedByUserId, dto);
            return Ok(new { message = "تم إرسال الدعوة بنجاح." });
        }

        /// <summary>
        /// Accepts an invitation using a token, allowing the invited user to join the system. 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequestDto dto)
        {
            var result = await _invitationService.AcceptInvitationAsync(dto);
            return Ok(result);
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
