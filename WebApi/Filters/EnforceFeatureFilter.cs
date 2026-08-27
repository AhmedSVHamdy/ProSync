using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters
{
    public class EnforceFeatureFilter : IAsyncActionFilter
    {
        private readonly string _featureName;
        private readonly ISubscriptionService _subscriptionService;

        public EnforceFeatureFilter(string featureName, ISubscriptionService subscriptionService)
        {
            _featureName = featureName;
            _subscriptionService = subscriptionService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var tenantIdClaim = context.HttpContext.User.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "لم يتم التعرف على الشركة." });
                return;
            }

            // هنا بالظبط بيتستخدم الكاش — فاكر ليه محتاجين الـ Subscription تتقرا بسرعة؟ ده السبب
            var subscription = await _subscriptionService.GetSubscriptionAsync(tenantId);

            var isAllowed = _featureName switch
            {
                "GitHubIntegration" => subscription.GitHubEnabled,
                _ => false   // أي Feature مش متعرفة، نرفضها احتياطياً (Fail-safe)
            };

            if (!isAllowed)
            {
                context.Result = new ObjectResult(new
                {
                    statusCode = 403,
                    message = $"هذه الميزة ({_featureName}) غير متاحة في باقتك الحالية. يرجى الترقية للاستفادة منها."
                })
                { StatusCode = 403 };
                return;
            }

            await next();   // كل حاجة تمام، خلي الكود يكمل للـ Action الحقيقي
        }
    }
}
