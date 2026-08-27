using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetProjectById
{
    public class GetSprintsByProjectQuery : IRequest<List<SprintResponseDto>>
    {
        public Guid ProjectId { get; set; }
    }
}
