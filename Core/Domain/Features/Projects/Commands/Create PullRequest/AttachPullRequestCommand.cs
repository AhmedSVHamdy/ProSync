using Core.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Features.Projects.Commands.Create_PullRequest
{
    public class AttachPullRequestCommand : IRequest<TaskItemResponseDto>
    {
        public Guid Id { get; set; }
        public string PullRequestUrl { get; set; } = string.Empty;
    }
}
