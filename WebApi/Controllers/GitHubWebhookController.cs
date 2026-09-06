using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/webhooks/github")]
    public class GitHubWebhookController : ControllerBase
    {
        private readonly IGitHubWebhookService _webhookService;
        private readonly IConfiguration _configuration;

        public GitHubWebhookController(IGitHubWebhookService webhookService, IConfiguration configuration)
        {
            _webhookService = webhookService;
            _configuration = configuration;
        }
        /// <summary>
        /// Handles incoming GitHub webhook events, verifies the signature, and processes pull request and pull request review events.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var signatureHeader = Request.Headers["X-Hub-Signature-256"].ToString();
            if (string.IsNullOrEmpty(signatureHeader))
                return Unauthorized(new { message = "التوقيع مفقود." });

            var secret = _configuration["GitHub:WebhookSecret"]!;
            if (!VerifySignature(rawBody, signatureHeader, secret))   // ← لازم تكون شايفها من هنا
                return Unauthorized(new { message = "التوقيع غير صالح." });

            var eventType = Request.Headers["X-GitHub-Event"].ToString();

            switch (eventType)
            {
                case "pull_request":
                    await _webhookService.HandlePullRequestEventAsync(rawBody);
                    break;
                case "pull_request_review":
                    await _webhookService.HandlePullRequestReviewEventAsync(rawBody);
                    break;
            }

            return Ok();
        }

        // ⚠️ تأكد إن الـ Method دي لسه هنا، جوه نفس الكلاس، مش اتشالت بالغلط
        private bool VerifySignature(string payload, string signatureHeader, string secret)
        {
            var expectedPrefix = "sha256=";
            if (!signatureHeader.StartsWith(expectedPrefix))
                return false;

            var receivedSignature = signatureHeader[expectedPrefix.Length..];

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(computedHash).ToLower();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(receivedSignature));
        }
    }
}
