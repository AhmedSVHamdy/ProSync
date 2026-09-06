using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DTO
{
    public class TaskBreakdownPreviewDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public Guid? SuggestedAssigneeId { get; set; }   // ← دلوقتي بيرجع الـ Id، مش الاسم بس
        public string? SuggestedAssigneeName { get; set; }
    }
}
