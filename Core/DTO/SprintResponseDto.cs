using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class SprintResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public Guid ProjectId { get; set; }
    }
}
