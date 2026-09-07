using Core.Domain.Features.Projects.Commands.BreakdownProjectTasks;
using Core.Domain.Features.Projects.Commands.Create_PullRequest;
using Core.Domain.Features.Projects.Commands.CreateTaskItem;
using Core.Domain.Features.Projects.Commands.ReassignTask;
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
        /// Creates a new task item for the current tenant.
        /// </summary>
        /// <param name="command">Task creation details.</param>
        /// <returns>Returns the created task item result.</returns>
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTaskItemCommand command)
        {
            command.TenantId = GetCurrentTenantId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Updates the status of a task item identified by the specified id.
        /// </summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="command">Contains the new status and related metadata.</param>
        /// <returns>Returns the updated task item result.</returns>
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
        /// Attaches a pull request to the specified task item.
        /// </summary>
        /// <param name="id">The ID of the task to attach the pull request to.</param>
        /// <param name="command">Pull request details to attach.</param>
        /// <returns>Returns the task item with the attached pull request.</returns>
        [HttpPut("{id}/pull-request")]
        public async Task<IActionResult> AttachPullRequest(Guid id, [FromBody] AttachPullRequestCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves all task items for the specified project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>Returns a list of task items belonging to the project.</returns>
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            var result = await _mediator.Send(new GetTasksByProjectQuery { ProjectId = projectId });
            return Ok(result);
        }

        /// <summary>
        /// Breaks down project work into task items according to the provided breakdown command.
        /// </summary>
        /// <param name="command">Details for breaking down the project into tasks.</param>
        /// <returns>Returns the result of the breakdown operation.</returns>
        [HttpPost("breakdown")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> BreakdownProjectTasks([FromBody] BreakdownProjectTasksCommand command)
        {
            command.TenantId = GetCurrentTenantId();
            command.RequestedByUserId = GetCurrentUserId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Reassigns a task item to a new assignee.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}/reassign")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Reassign(Guid id, [FromBody] ReassignTaskCommand command)
        {
            command.Id = id;
            command.TenantId = GetCurrentTenantId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
