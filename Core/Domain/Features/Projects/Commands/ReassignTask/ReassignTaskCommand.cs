using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.ReassignTask
{
    public class ReassignTaskCommand : IRequest<TaskItemResponseDto>
    {
        public Guid Id { get; set; }
        public Guid NewAssigneeId { get; set; }

        [JsonIgnore]
        public Guid TenantId { get; set; }
    }
}
