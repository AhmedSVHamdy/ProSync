using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class TaskItem : TenantEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;  
        public string PullRequestUrl { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime LastActivityAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid? SprintId { get; set; }       
        public Sprint? Sprint { get; set; }

        public Guid AssigneeId { get; set; }
        public User Assignee { get; set; } = null!;
    }
}
