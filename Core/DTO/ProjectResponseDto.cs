using Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
   public class ProjectResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public Guid TenantId { get; set; }
    }
}
