using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.CloseSprint
{
    public class CloseSprintCommand : IRequest<SprintSummaryDto>
    {
        public Guid Id { get; set; }
    }
}
