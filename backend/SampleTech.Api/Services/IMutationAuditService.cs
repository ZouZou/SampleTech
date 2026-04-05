using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record MutationAuditContext(
    Guid TenantId,
    string EntityType,
    Guid EntityId,
    MutationAction Action,
    Guid? ActorUserId = null,
    string? ActorRole = null,
    string? PreviousState = null,
    string? NextState = null,
    string? IpAddress = null,
    string? UserAgent = null);

public interface IMutationAuditService
{
    Task LogAsync(MutationAuditContext ctx, CancellationToken ct = default);
}
