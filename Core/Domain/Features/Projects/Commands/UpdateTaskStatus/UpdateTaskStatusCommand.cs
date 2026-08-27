using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.UpdateTaskStatus
{
    public class UpdateTaskStatusCommand : IRequest<TaskItemResponseDto>
    {
        public Guid Id { get; set; }
        public Core.Enums.TaskStatus NewStatus { get; set; }

        [JsonIgnore]
        public Guid CurrentUserId { get; set; }   // بيتحط من الكونترولر، مش من اليوزر

        [JsonIgnore]
        public string CurrentUserRole { get; set; } = string.Empty;   // نفس الكلام
    }
}
