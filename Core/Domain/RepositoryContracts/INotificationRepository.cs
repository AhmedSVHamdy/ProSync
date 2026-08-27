using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
    }
}
