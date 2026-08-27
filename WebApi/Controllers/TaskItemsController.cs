using Core.Domain.Features.Projects.Commands.Create_PullRequest;
using Core.Domain.Features.Projects.Commands.CreateTaskItem;
using Core.Domain.Features.Projects.Commands.UpdateTaskStatus;
using Core.Domain.Features.Projects.Queries.GetProjectById;
using Core.Domain.Features.Projects.Queries.GetTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TaskItemsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public TaskItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Create TaskIem by Owner or Admin
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTaskItemCommand command)
        {
            command.TenantId = GetCurrentTenantId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Update Status by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusCommand command)
        {
            command.Id = id;
            command.CurrentUserId = GetCurrentUserId();
            command.CurrentUserRole = GetCurrentUserRole();   // Method جديدة محتاجين نضيفها
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Update Pull Request by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}/pull-request")]
        public async Task<IActionResult> AttachPullRequest(Guid id, [FromBody] AttachPullRequestCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Get Project by ID
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            var result = await _mediator.Send(new GetTasksByProjectQuery { ProjectId = projectId });
            return Ok(result);
        }
    }
}
