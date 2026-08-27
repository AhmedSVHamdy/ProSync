using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class SubscriptionResponseDto
    {
        public Guid Id { get; set; }
        public string PlanTier { get; set; } = string.Empty;
        public int MaxEmployees { get; set; }
        public bool GitHubEnabled { get; set; }
    }
}
