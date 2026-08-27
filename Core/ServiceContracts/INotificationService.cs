using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Guid userId, Guid tenantId, NotificationType type, string title, string message, Guid? taskItemId = null);
    }
}
