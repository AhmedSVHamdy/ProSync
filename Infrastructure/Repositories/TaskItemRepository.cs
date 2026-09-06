using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Core.Enums;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class TaskItemRepository : GenericRepository<TaskItem>, ITaskItemRepository
    {
        public TaskItemRepository(ProSyncContext context) : base(context) { }

        public async Task<TaskItem?> GetByIdWithAssigneeAsync(Guid id)
        {
            return await _dbSet
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TaskItem>> GetByProjectIdWithAssigneeAsync(Guid projectId)
        {
            return await _dbSet
                .Include(t => t.Assignee)
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetBySprintIdAsync(Guid sprintId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(t => t.SprintId == sprintId)
                .ToListAsync();
        }
        public async Task<List<TaskItem>> GetOverdueCriticalTasksAsync(int hoursThreshold)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hoursThreshold);

            return await _dbSet
                .IgnoreQueryFilters()   // ← الإضافة المهمة
                .AsNoTracking()
                .Where(t =>
                    t.Priority == TaskPriority.Critical &&
                    t.Status != Core.Enums.TaskStatus.Done &&
                    t.LastActivityAt < cutoffTime)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByPullRequestUrlAsync(string pullRequestUrl)
        {
            return await _dbSet
                .IgnoreQueryFilters()   // ← فاكر ليه؟ نفس السبب بالظبط — الـ Webhook مش HTTP Request عادي فيه JWT، فمفيش Tenant Context
                .FirstOrDefaultAsync(t => t.PullRequestUrl == pullRequestUrl);
        }
    }
}
