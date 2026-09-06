using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.BreakdownProjectTasks
{
    public class BreakdownProjectTasksCommand : IRequest<List<TaskItemResponseDto>>
    {
        public Guid ProjectId { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<Guid> TeamMemberIds { get; set; } = new();

        [JsonIgnore]
        public Guid TenantId { get; set; }

        [JsonIgnore]
        public Guid RequestedByUserId { get; set; }   // فاكر ليه؟ هو اللي هيتعين مبدئياً على التاسكات
    }
}
