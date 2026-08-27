using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.Create_Sprint
{
    public class CreateSprintCommand : IRequest<SprintResponseDto>
    {
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid ProjectId { get; set; }

        [JsonIgnore]
        public Guid TenantId { get; set; }   // نفس الدرس اللي اتعلمناه من Project
    }
}
