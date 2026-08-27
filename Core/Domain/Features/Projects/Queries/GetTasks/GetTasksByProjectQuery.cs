using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Queries.GetTasks
{
    public class GetTasksByProjectQuery : IRequest<List<TaskItemResponseDto>>
    {
        public Guid ProjectId { get; set; }
    }
}
