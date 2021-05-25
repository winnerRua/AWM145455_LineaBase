using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using WSREGGWMM.Entities;

namespace WSREGGWMM.Helpers
{
    public class CustomAuthorizeFilter : IAsyncAuthorizationFilter
    {
        public AuthorizationPolicy Policy { get; }

        public CustomAuthorizeFilter(AuthorizationPolicy policy)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Allow Anonymous skips all authorization
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
                return;

            var policyEvaluator = context.HttpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();
            var authenticateResult = await policyEvaluator.AuthenticateAsync(Policy, context.HttpContext);
            var authorizeResult = await policyEvaluator.AuthorizeAsync(Policy, authenticateResult, context.HttpContext, context);

            if (authorizeResult.Challenged)
                context.Result = new CustomResult("Authorization failed.", StatusCodes.Status401Unauthorized);
            else if (authorizeResult.Forbidden)
                context.Result = new CustomResult("Authorization failed.", StatusCodes.Status403Forbidden);

        }
    }
}
