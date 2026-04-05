using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record AuditEventContext(
    AuthEventType EventType,
    string? Email = null,
    Guid? UserId = null,
    Guid? TenantId = null,
    string? IpAddress = null,
    string? UserAgent = null,
    string? Metadata = null);

public interface IAuditService
{
    Task LogAsync(AuditEventContext ctx, CancellationToken ct = default);
}
