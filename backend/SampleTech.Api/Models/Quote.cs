namespace SampleTech.Api.Models;

public enum QuoteStatus
{
    Draft,
    Issued,
    Accepted,
    Declined,
    Expired,
    Superseded
}

public class Quote
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public Guid IssuedByUserId { get; set; }
    public User IssuedByUser { get; set; } = null!;

    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

    /// <summary>Increments on re-rate; starts at 1.</summary>
    public int Version { get; set; } = 1;

    public decimal TotalPremium { get; set; }
    public decimal Taxes { get; set; }
    public decimal Fees { get; set; }
    public decimal TotalDue { get; set; }

    public DateOnly EffectiveDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public DateOnly QuoteExpiryDate { get; set; }

    /// <summary>Denormalized coverage summary JSON for display.</summary>
    public string Coverages { get; set; } = "[]";

    public string? Terms { get; set; }
    public DateTimeOffset? BindRequestedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Set when this quote results in a bound policy.</summary>
    public Policy? Policy { get; set; }
}
