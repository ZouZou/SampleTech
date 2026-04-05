namespace SampleTech.Api.Models;

public enum AuthEventType
{
    LoginSuccess,
    LoginFailed,
    Logout,
    TokenRefreshed,
    TokenRefreshFailed,
    AccountLocked
}

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AuthEventType EventType { get; set; }

    /// <summary>Null for events where the user cannot be resolved (e.g. LoginFailed with unknown email).</summary>
    public Guid? UserId { get; set; }

    /// <summary>Email provided during the auth attempt; useful for failed logins.</summary>
    public string? Email { get; set; }

    public Guid? TenantId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Optional JSON payload for additional context (e.g. failure reason).</summary>
    public string? Metadata { get; set; }
}
