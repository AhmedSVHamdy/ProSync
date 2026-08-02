using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Tenant:BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string PlanType { get; set; } = string.Empty;

    }
}
