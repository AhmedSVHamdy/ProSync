using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.AiTasks.ConfirmBreakdown
{
    public class ConfirmBreakdownCommand : IRequest<List<TaskItemResponseDto>>
    {
        public Guid ProjectId { get; set; }
        public List<ConfirmedTaskItemDto> Tasks { get; set; } = new();

        [JsonIgnore]
        public Guid TenantId { get; set; }

        [JsonIgnore]
        public Guid RequestedByUserId { get; set; }
    }

    
}
