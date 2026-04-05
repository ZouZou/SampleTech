using System.Text.Json;

namespace SampleTech.Api.Models;

public enum InsuredType
{
    Individual,
    Business
}

public class Insured
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public InsuredType Type { get; set; }

    // Individual fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    // Business fields
    public string? BusinessName { get; set; }

    /// <summary>EIN or SSN — must be encrypted at rest (AES-256). TODO: Phase 2 KMS integration.</summary>
    public string? TaxId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    /// <summary>JSON: {street, city, state, zip, country}</summary>
    public string Address { get; set; } = "{}";

    /// <summary>Set when Agent grants client portal access to this insured.</summary>
    public Guid? LinkedUserId { get; set; }
    public User? LinkedUser { get; set; }

    public Guid? AssignedAgentId { get; set; }
    public User? AssignedAgent { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Submission> Submissions { get; set; } = [];
    public ICollection<Policy> Policies { get; set; } = [];
}
