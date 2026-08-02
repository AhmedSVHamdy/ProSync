using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Project : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
