namespace SampleTech.Api.Authorization;

/// <summary>
/// Scoped service that carries the resolved tenant for the current request.
/// Populated by TenantMiddleware after JWT validation.
/// </summary>
public class TenantContext
{
    /// <summary>
    /// The tenant ID resolved from the authenticated user's JWT claim.
    /// Null for platform Admin users who are not scoped to a tenant.
    /// </summary>
    public Guid? TenantId { get; set; }

    public bool HasTenant => TenantId.HasValue;
}
