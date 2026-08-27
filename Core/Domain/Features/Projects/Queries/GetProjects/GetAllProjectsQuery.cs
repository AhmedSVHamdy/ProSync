using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetAllProjects
{
    public class GetAllProjectsQuery : IRequest<List<ProjectResponseDto>>
    {
        public Guid TenantId { get; set; }
    }
}
