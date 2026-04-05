namespace SampleTech.Api.Models;

public enum SubmissionStatus
{
    Draft,
    Submitted,
    InReview,
    Quoted,
    Bound,
    Declined,
    Cancelled
}

public enum LineOfBusiness
{
    Auto,
    Property,
    GeneralLiability,
    WorkersComp,
    Umbrella
}

public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid InsuredId { get; set; }
    public Insured Insured { get; set; } = null!;

    public Guid SubmittedByUserId { get; set; }
    public User SubmittedByUser { get; set; } = null!;

    public Guid? AssignedUnderwriterId { get; set; }
    public User? AssignedUnderwriter { get; set; }

    public LineOfBusiness LineOfBusiness { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;

    public DateOnly EffectiveDate { get; set; }
    public DateOnly ExpirationDate { get; set; }

    /// <summary>Line-of-business-specific risk attributes (JSON).</summary>
    public string RiskData { get; set; } = "{}";

    public string? Notes { get; set; }
    public string? DeclineReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Quote> Quotes { get; set; } = [];
}
