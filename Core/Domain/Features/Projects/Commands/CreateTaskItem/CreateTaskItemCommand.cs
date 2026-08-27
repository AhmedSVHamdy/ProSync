using Core.DTO;
using Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.CreateTaskItem
{
    public class CreateTaskItemCommand : IRequest<TaskItemResponseDto>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? SprintId { get; set; }
        public Guid AssigneeId { get; set; }

        [JsonIgnore]
        public Guid TenantId { get; set; }   // نفس القاعدة الثابتة اللي اتفقنا عليها
    }
}
