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
        /// <summary>
        /// This endpoint is protected by the "GitHubIntegration" feature flag. Only users whose companies have this feature enabled can access it.
        /// </summary>
        /// <returns></returns>
        [HttpGet("github-only")]
        [EnforceFeature("GitHubIntegration")]
        public IActionResult GitHubOnlyEndpoint()
        {
            return Ok(new { message = "وصلت للـ Endpoint! معناها شركتك عندها GitHubIntegration مفعّلة." });
        }
    }
}
