using System.Security.Claims;
using SampleTech.Api.Authorization;

namespace SampleTech.Api.Middleware;

/// <summary>
/// Resolves the tenant ID from the authenticated user's JWT and stores it
/// in the scoped <see cref="TenantContext"/> for downstream use.
/// Must run after UseAuthentication().
/// </summary>
public class TenantMiddleware(RequestDelegate next)
{
    private const string TenantIdClaimType = "tenant_id";

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirstValue(TenantIdClaimType);
            if (Guid.TryParse(tenantClaim, out var tenantId))
                tenantContext.TenantId = tenantId;
        }

        await next(context);
    }
}
