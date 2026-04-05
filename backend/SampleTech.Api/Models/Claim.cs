namespace SampleTech.Api.Models;

public enum ClaimStatus
{
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    Closed
}

public class Claim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClaimNumber { get; set; } = string.Empty;

    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public Guid ClaimantId { get; set; }
    public User Claimant { get; set; } = null!;

    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }

    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
    public string Description { get; set; } = string.Empty;
    public decimal ClaimedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }

    public DateOnly IncidentDate { get; set; }
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ReviewNotes { get; set; }
}
