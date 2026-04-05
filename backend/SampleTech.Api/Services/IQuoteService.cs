using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record QuoteSummaryDto(
    Guid Id,
    Guid SubmissionId,
    QuoteStatus Status,
    int Version,
    decimal TotalPremium,
    decimal TotalDue,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    DateOnly QuoteExpiryDate,
    Guid IssuedByUserId,
    DateTimeOffset CreatedAt);

public record QuoteDetailDto(
    Guid Id,
    Guid SubmissionId,
    QuoteStatus Status,
    int Version,
    decimal TotalPremium,
    decimal Taxes,
    decimal Fees,
    decimal TotalDue,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    DateOnly QuoteExpiryDate,
    string Coverages,
    string? Terms,
    DateTimeOffset? BindRequestedAt,
    Guid IssuedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateQuoteRequest(
    Guid SubmissionId,
    decimal TotalPremium,
    decimal Taxes,
    decimal Fees,
    DateOnly QuoteExpiryDate,
    string? Coverages,
    string? Terms);

public record TransitionQuoteStatusRequest(
    QuoteStatus NewStatus);

public record RatePreviewRequest(Guid SubmissionId);

public interface IQuoteService
{
    Task<IReadOnlyList<QuoteSummaryDto>> ListBySubmissionAsync(Guid submissionId, Guid tenantId, CancellationToken ct = default);
    Task<QuoteDetailDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<QuoteDetailDto> CreateAsync(CreateQuoteRequest request, Guid tenantId, Guid underwriterId, UserRole actorRole, CancellationToken ct = default);
    Task<QuoteDetailDto?> TransitionStatusAsync(Guid id, TransitionQuoteStatusRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);

    /// <summary>
    /// Runs the rating engine against a submission and returns the computed premium breakdown
    /// without persisting anything. Used for previewing before committing a quote.
    /// </summary>
    Task<RatingResult> RatePreviewAsync(RatePreviewRequest request, Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Re-applies the current active rate table to a quote that has not yet been bound
    /// (status must be Draft or Issued). Increments the quote version.
    /// </summary>
    Task<QuoteDetailDto?> RerateAsync(Guid id, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
}
