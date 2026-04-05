namespace SampleTech.Api.Models;

public enum PolicyStatus
{
    /// <summary>Policy record created but not yet quoted.</summary>
    Draft,
    /// <summary>A quote has been issued against this policy.</summary>
    Quoted,
    /// <summary>Quote accepted; terms locked. Awaiting effective date.</summary>
    Bound,
    /// <summary>Policy is in force.</summary>
    Active,
    /// <summary>Policy expiration date has passed.</summary>
    Expired,
    /// <summary>Policy cancelled by underwriter or admin.</summary>
    Cancelled
}

public class Policy
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    /// <summary>The quote that produced this policy.</summary>
    public Guid? QuoteId { get; set; }
    public Quote? Quote { get; set; }

    public Guid InsuredId { get; set; }
    public Insured Insured { get; set; } = null!;

    /// <summary>Human-readable, unique per tenant. Format: {TENANT_CODE}-{LOB_CODE}-{YEAR}-{SEQ}.</summary>
    public string PolicyNumber { get; set; } = string.Empty;

    public LineOfBusiness LineOfBusiness { get; set; }
    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    public DateOnly EffectiveDate { get; set; }
    public DateOnly ExpirationDate { get; set; }

    public decimal TotalPremium { get; set; }

    public Guid IssuedByUserId { get; set; }
    public User IssuedByUser { get; set; } = null!;

    public Guid? AssignedAgentId { get; set; }
    public User? AssignedAgent { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    /// <summary>Self-reference to the successor policy on renewal.</summary>
    public Guid? RenewalPolicyId { get; set; }
    public Policy? RenewalPolicy { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Coverage> Coverages { get; set; } = [];
    public ICollection<PolicyDocument> Documents { get; set; } = [];
    public ICollection<Claim> Claims { get; set; } = [];
}
