using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetProjects
{
    public class GetProjectByIdQuery : IRequest<ProjectResponseDto>
    {
        public Guid Id { get; set; }
    }
}
