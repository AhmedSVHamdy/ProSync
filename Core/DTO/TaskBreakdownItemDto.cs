using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class TaskBreakdownItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string? SuggestedAssigneeName { get; set; }
    }
}
