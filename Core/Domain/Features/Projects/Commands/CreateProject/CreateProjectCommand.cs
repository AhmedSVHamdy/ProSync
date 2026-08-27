using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Core.Domain.Features.Projects.Commands.CreateProject
{
    public class CreateProjectCommand : IRequest<ProjectResponseDto>
    {
        public string Name { get; set; } = string.Empty;
        [JsonIgnore]
        public Guid TenantId { get; set; }   // بيتحط من الـ Controller (من الـ JWT)، مش من اليوزر مباشرة
    }
}
