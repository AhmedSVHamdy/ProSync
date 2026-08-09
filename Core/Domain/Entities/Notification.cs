using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Notification :TenantEntity
    {
        
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? Payload { get; set; }
        public string? GroupKey { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid? TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
    }
}
