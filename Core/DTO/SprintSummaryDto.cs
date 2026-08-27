using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class SprintSummaryDto
    {
        public Guid SprintId { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int IncompleteTasks { get; set; }
        public double CompletionPercentage { get; set; }
    }
}
