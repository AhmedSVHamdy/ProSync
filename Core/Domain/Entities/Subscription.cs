using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Subscription : TenantEntity
    {
        public string PlanTier { get; set; } = string.Empty;
        public int MaxEmployees { get; set; } 
        public bool GitHubEnabled { get; set; } 

    }
}
