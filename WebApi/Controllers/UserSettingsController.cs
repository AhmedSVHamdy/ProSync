using Core.Domain.Features.Projects.Commands.UpdateUserSettings;
using Core.Domain.RepositoryContracts;
using Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/user-settings")]
    [Authorize]
    public class UserSettingsController : BaseApiController
    {
        private readonly IUserSettingsRepository _userSettingsRepository;

        public UserSettingsController(IUserSettingsRepository userSettingsRepository)
        {
            _userSettingsRepository = userSettingsRepository;
        }
        /// <summary>
        /// Get UserSetting by Authorize
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetMySettings()
        {
            var userId = GetCurrentUserId();
            var settings = await _userSettingsRepository.GetByUserIdAsync(userId);

            if (settings is null)
            {
                return Ok(new UserSettingsResponseDto
                {
                    EmailNotifications = true,
                    NotificationsEnabled = true
                });
            }

            return Ok(new UserSettingsResponseDto
            {
                EmailNotifications = settings.EmailNotifications,
                NotificationsEnabled = settings.NotificationsEnabled
            });
        }
        /// <summary>
        /// Update UserSetting by Authorize
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateUserSettingsCommand command)
        {
            var userId = GetCurrentUserId();
            var settings = await _userSettingsRepository.GetByUserIdAsync(userId)
                ?? throw new InvalidOperationException("الإعدادات غير موجودة.");

            settings.EmailNotifications = command.EmailNotifications;
            settings.NotificationsEnabled = command.NotificationsEnabled;

            await _userSettingsRepository.UpdateAsync(settings);
            return Ok(settings);
        }
    }
}
