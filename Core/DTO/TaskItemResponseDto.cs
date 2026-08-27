using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class TaskItemResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Core.Enums.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public string PullRequestUrl { get; set; } = string.Empty;
        public DateTime LastActivityAt { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? SprintId { get; set; }
        public Guid AssigneeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;   // مفيدة للـ Kanban Board في الـ Frontend
    }
}
