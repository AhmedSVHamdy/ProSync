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
       /// Create Sprint 
       /// </summary>
       /// <param name="command"></param>
       /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSprintCommand command)
        {
            command.TenantId = GetCurrentTenantId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Update Close
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}/close")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Close(Guid id)
        {
            var result = await _mediator.Send(new CloseSprintCommand { Id = id });
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
            var result = await _mediator.Send(new GetSprintsByProjectQuery { ProjectId = projectId });
            return Ok(result);
        }
    }
}
