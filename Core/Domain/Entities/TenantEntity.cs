using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class TenantEntity :BaseEntity
    {
        public Guid TenantId { get; set; }
    }
}
