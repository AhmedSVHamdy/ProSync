using Core.Domain.Features.Projects.Commands.UpdateSubscription;
using Core.Domain.Features.Projects.Queries.GetSubscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/subscription")]
    [Authorize]
    public class SubscriptionsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public SubscriptionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new GetSubscriptionQuery { TenantId = GetCurrentTenantId() });
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Owner")]   // فاكر ليه Owner بس؟ قرارات الفلوس والباقات بتاعة صاحب الشركة
        public async Task<IActionResult> Update([FromBody] UpdateSubscriptionCommand command)
        {
            command.TenantId = GetCurrentTenantId();
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
