using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Project : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; } 
    }
}
