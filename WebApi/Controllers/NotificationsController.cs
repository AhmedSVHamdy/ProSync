using Core.Domain.RepositoryContracts;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        /// <summary>
        /// Get unread notifications for the current user.
        /// </summary>
        /// <returns></returns>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var result = await _notificationRepository.GetUnreadByUserIdAsync(GetCurrentUserId());
            return Ok(result);
        }
        /// <summary>
        /// Mark a notification as read for the current user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationRepository.MarkAsReadAsync(id);
            return Ok(new { message = "تم تحديد الإشعار كمقروء." });
        }

      
    }
}
