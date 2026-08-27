using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.DeleteProject
{
    public class DeleteProjectCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}
