using Core.DTO;
using Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.UpdateProject
{
    public class UpdateProjectCommand : IRequest<ProjectResponseDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
    }
}
