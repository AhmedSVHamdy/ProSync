using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IGitHubWebhookService
    {
        Task HandlePullRequestEventAsync(string rawPayload);
        Task HandlePullRequestReviewEventAsync(string rawPayload);
    }
}
