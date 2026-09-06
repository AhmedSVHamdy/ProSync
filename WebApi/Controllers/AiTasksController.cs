using Core.Domain.Features.Projects.Commands.AiTasks.ConfirmBreakdown;
using Core.Domain.Features.Projects.Commands.AiTasks.PreviewBreakdown;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/ai-tasks")]
    public class AiTasksController : BaseApiController
    {
        private readonly IMediator _mediator;

        public AiTasksController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Generates a preview of the AI-based task breakdown for the current tenant.
        /// </summary>
        /// <param name="command">The request containing the task details used to generate the breakdown preview.</param>
        /// <returns>A successful response containing the generated breakdown preview.</returns>
        [HttpPost("breakdown/preview")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> PreviewBreakdown([FromBody] PreviewBreakdownCommand command)
        {
            command.TenantId = GetCurrentTenantId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Confirms the AI-generated task breakdown and creates the corresponding sub-tasks for the current user and tenant.
        /// </summary>
        /// <param name="command">The confirmation request containing the breakdown details and the user who is confirming it.</param>
        /// <returns>A successful response containing the confirmed breakdown result.</returns>
        [HttpPost("breakdown/confirm")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> ConfirmBreakdown([FromBody] ConfirmBreakdownCommand command)
        {
            command.TenantId = GetCurrentTenantId();
            command.RequestedByUserId = GetCurrentUserId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
