namespace SampleTech.Api.Models;

public enum MutationAction
{
    Create,
    Update,
    Delete,
    StatusChange
}

/// <summary>
/// Immutable domain mutation audit trail.
/// Tracks all create/update/delete/status changes on domain entities (Policy, Quote, Submission, Insured, Coverage).
/// Append-only — never updated or deleted.
/// </summary>
public class MutationAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    /// <summary>Null for system-triggered actions (cron jobs, etc.).</summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>Role of the actor at the time of the action.</summary>
    public string? ActorRole { get; set; }

    /// <summary>Domain entity type name, e.g. "Policy", "Quote", "Submission".</summary>
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public MutationAction Action { get; set; }

    /// <summary>JSON snapshot of entity state before the mutation. Null for creates.</summary>
    public string? PreviousState { get; set; }

    /// <summary>JSON snapshot of entity state after the mutation. Null for deletes.</summary>
    public string? NextState { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
