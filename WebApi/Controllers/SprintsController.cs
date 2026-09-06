using Core.Domain.Features.Projects.Commands.CloseSprint;
using Core.Domain.Features.Projects.Commands.Create_Sprint;
using Core.Domain.Features.Projects.Queries.GetProjectById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/sprints")]
    [Authorize]
    public class SprintsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public SprintsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Creates a new sprint for the current tenant.
        /// </summary>
        /// <param name="command">The command containing sprint creation details.</param>
        /// <returns>Returns the result of the created sprint.</returns>
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSprintCommand command)
        {
            command.TenantId = GetCurrentTenantId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Closes the sprint identified by the given id.
        /// </summary>
        /// <param name="id">The identifier of the sprint to close.</param>
        /// <returns>Returns the result of the close operation.</returns>
        [HttpPut("{id}/close")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Close(Guid id)
        {
            var result = await _mediator.Send(new CloseSprintCommand { Id = id });
            return Ok(result);
        }
        /// <summary>
        /// Retrieves all sprints for the specified project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>Returns a list of sprints for the project.</returns>
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            var result = await _mediator.Send(new GetSprintsByProjectQuery { ProjectId = projectId });
            return Ok(result);
        }
    }
}
