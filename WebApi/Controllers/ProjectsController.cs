using Core.Domain.Features.Projects.Commands.CreateProject;
using Core.Domain.Features.Projects.Commands.DeleteProject;
using Core.Domain.Features.Projects.Commands.UpdateProject;
using Core.Domain.Features.Projects.Queries.GetAllProjects;
using Core.Domain.Features.Projects.Queries.GetProjectById;
using Core.Domain.Features.Projects.Queries.GetProjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    /// <summary>
    /// Controller for managing projects. All actions require authentication and are scoped to the current tenant.
    /// </summary>
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : BaseApiController   // فاكر الـ Base بتاعة GetCurrentUserId؟
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// Create a new project for the current tenant.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProjectCommand command)
        {
            command.TenantId = GetCurrentTenantId();   // Method جديدة هنضيفها في BaseApiController
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Update an existing project by its ID. The project must belong to the current tenant.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        /// <summary>
        /// Delete (archive) a project by its ID. The project must belong to the current tenant.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteProjectCommand { Id = id });
            return Ok(new { message = "تم أرشفة المشروع بنجاح." });
        }
        /// <summary>
        /// Get a project by its ID. The project must belong to the current tenant.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetProjectByIdQuery { Id = id });
            return Ok(result);
        }
        /// <summary>
        /// Get all projects for the current tenant.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllProjectsQuery { TenantId = GetCurrentTenantId() });
            return Ok(result);
        }
    }
}
