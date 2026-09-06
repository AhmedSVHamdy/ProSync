using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.AiTasks.PreviewBreakdown
{
    public class PreviewBreakdownCommand : IRequest<List<TaskBreakdownPreviewDto>>
    {
        public Guid ProjectId { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<Guid> TeamMemberIds { get; set; } = new();

        [JsonIgnore]
        public Guid TenantId { get; set; }
    }
}
