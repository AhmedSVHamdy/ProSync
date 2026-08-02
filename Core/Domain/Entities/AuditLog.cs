using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class AuditLog : TenantEntity
    {
        public string Action { get; set; } = string.Empty;       
        public DateTime Timestamp { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } 
        public Guid TaskItemId { get; set; }    
        public TaskItem TaskItem { get; set; } 
    }
}
