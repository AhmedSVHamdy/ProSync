using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.Enums;
using Core.ServiceContracts;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly IUserRepository _userRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserSettingsRepository userSettingsRepository,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _userSettingsRepository = userSettingsRepository;
            _userRepository = userRepository;
        }

        public async Task CreateNotificationAsync(
    Guid userId, Guid tenantId, NotificationType type, string title, string message, Guid? taskItemId = null)
        {
            var settings = await _userSettingsRepository.GetByUserIdAsync(userId);

            if (settings is not null && !settings.NotificationsEnabled)
                return;

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                TaskItemId = taskItemId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);

            // الإضافة الجديدة: لو اليوزر مفعّل الإيميل، ابعتله كمان
            if (settings is null || settings.EmailNotifications)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user is not null)
                {
                    BackgroundJob.Enqueue<IEmailService>(x => x.SendNotificationEmailAsync(user.Email, title, message));
                }
            }
        }
    }
}
