using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.RepositoryContracts
{
    public interface ITaskItemRepository : IGenericRepository<TaskItem>
    {
        Task<TaskItem?> GetByIdWithAssigneeAsync(Guid id);
        Task<List<TaskItem>> GetByProjectIdWithAssigneeAsync(Guid projectId);
        Task<List<TaskItem>> GetBySprintIdAsync(Guid sprintId);   // اللي كانت Placeholder في CloseSprintCommandHandler
        Task<List<TaskItem>> GetOverdueCriticalTasksAsync(int hoursThreshold);
        Task<TaskItem?> GetByPullRequestUrlAsync(string pullRequestUrl);
    }
}
