using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.Enums;
using Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserSettingsRepository _userSettingsRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserSettingsRepository userSettingsRepository)
        {
            _notificationRepository = notificationRepository;
            _userSettingsRepository = userSettingsRepository;
        }

        public async Task CreateNotificationAsync(
            Guid userId, Guid tenantId, NotificationType type, string title, string message, Guid? taskItemId = null)
        {
            var settings = await _userSettingsRepository.GetByUserIdAsync(userId);

            // لو اليوزر قافل الإشعارات بالكامل، منسجلش حاجة أصلاً
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

            // TODO: لو settings.EmailNotifications == true، نبعت إيميل كمان (Hangfire Background Job)
        }
    }
}
