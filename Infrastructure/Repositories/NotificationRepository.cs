using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ProSyncContext context) : base(context) { }

        public async Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            await _dbSet
                .Where(n => n.Id == notificationId)
                .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true));
        }
    }
}
