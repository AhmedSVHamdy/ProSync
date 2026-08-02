using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Sprint: TenantEntity
    {
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; } = false;

        public Guid ProjectId { get; set; }
        public Project Project {  get; set; }
        
    }
}
