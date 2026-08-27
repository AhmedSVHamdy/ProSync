using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Filters;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/feature-test")]
    [Authorize]
    public class FeatureTestController : ControllerBase
    {
        [HttpGet("github-only")]
        [EnforceFeature("GitHubIntegration")]
        public IActionResult GitHubOnlyEndpoint()
        {
            return Ok(new { message = "وصلت للـ Endpoint! معناها شركتك عندها GitHubIntegration مفعّلة." });
        }
    }
}
