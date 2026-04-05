using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class QuoteService(AppDbContext db, IMutationAuditService auditService, IRatingEngine ratingEngine) : IQuoteService
{
    private static readonly IReadOnlyDictionary<QuoteStatus, IReadOnlySet<QuoteStatus>> AllowedTransitions =
        new Dictionary<QuoteStatus, IReadOnlySet<QuoteStatus>>
        {
            [QuoteStatus.Draft]      = new HashSet<QuoteStatus> { QuoteStatus.Issued },
            [QuoteStatus.Issued]     = new HashSet<QuoteStatus> { QuoteStatus.Accepted, QuoteStatus.Declined, QuoteStatus.Expired, QuoteStatus.Superseded },
            [QuoteStatus.Accepted]   = new HashSet<QuoteStatus>(),
            [QuoteStatus.Declined]   = new HashSet<QuoteStatus>(),
            [QuoteStatus.Expired]    = new HashSet<QuoteStatus>(),
            [QuoteStatus.Superseded] = new HashSet<QuoteStatus>()
        };

    public async Task<IReadOnlyList<QuoteSummaryDto>> ListBySubmissionAsync(
        Guid submissionId, Guid tenantId, CancellationToken ct = default)
    {
        return await db.Quotes.AsNoTracking()
            .Where(q => q.SubmissionId == submissionId && q.TenantId == tenantId)
            .OrderByDescending(q => q.Version)
            .Select(q => new QuoteSummaryDto(
                q.Id, q.SubmissionId, q.Status, q.Version,
                q.TotalPremium, q.TotalDue,
                q.EffectiveDate, q.ExpirationDate, q.QuoteExpiryDate,
                q.IssuedByUserId, q.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<QuoteDetailDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var q = await db.Quotes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return q is null ? null : MapDetail(q);
    }

    public async Task<QuoteDetailDto> CreateAsync(
        CreateQuoteRequest request, Guid tenantId, Guid underwriterId, UserRole actorRole,
        CancellationToken ct = default)
    {
        // Supersede any prior Issued quotes for the same submission
        var priorIssued = await db.Quotes
            .Where(q => q.SubmissionId == request.SubmissionId
                     && q.TenantId == tenantId
                     && q.Status == QuoteStatus.Issued)
            .ToListAsync(ct);

        foreach (var prior in priorIssued)
        {
            prior.Status = QuoteStatus.Superseded;
            prior.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // Determine version number
        var maxVersion = await db.Quotes.AsNoTracking()
            .Where(q => q.SubmissionId == request.SubmissionId && q.TenantId == tenantId)
            .MaxAsync(q => (int?)q.Version, ct) ?? 0;

        // Fetch submission to copy dates
        var submission = await db.Submissions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId && s.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException($"Submission {request.SubmissionId} not found.");

        var quote = new Quote
        {
            TenantId = tenantId,
            SubmissionId = request.SubmissionId,
            IssuedByUserId = underwriterId,
            Status = QuoteStatus.Draft,
            Version = maxVersion + 1,
            TotalPremium = request.TotalPremium,
            Taxes = request.Taxes,
            Fees = request.Fees,
            TotalDue = request.TotalPremium + request.Taxes + request.Fees,
            EffectiveDate = submission.EffectiveDate,
            ExpirationDate = submission.ExpirationDate,
            QuoteExpiryDate = request.QuoteExpiryDate,
            Coverages = request.Coverages ?? "[]",
            Terms = request.Terms
        };

        db.Quotes.Add(quote);
        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Quote",
            EntityId: quote.Id,
            Action: MutationAction.Create,
            ActorUserId: underwriterId,
            ActorRole: actorRole.ToString()), ct);

        return MapDetail(quote);
    }

    public async Task<QuoteDetailDto?> TransitionStatusAsync(
        Guid id, TransitionQuoteStatusRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var quote = await db.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.TenantId == tenantId, ct);
        if (quote is null) return null;

        if (!AllowedTransitions.TryGetValue(quote.Status, out var allowed) ||
            !allowed.Contains(request.NewStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition quote from {quote.Status} to {request.NewStatus}. " +
                $"Allowed: [{string.Join(", ", allowed ?? (IEnumerable<QuoteStatus>)[])}]");
        }

        quote.Status = request.NewStatus;
        quote.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.NewStatus == QuoteStatus.Accepted)
            quote.BindRequestedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Quote",
            EntityId: quote.Id,
            Action: MutationAction.StatusChange,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return MapDetail(quote);
    }

    private static readonly IReadOnlySet<QuoteStatus> RerateableStatuses =
        new HashSet<QuoteStatus> { QuoteStatus.Draft, QuoteStatus.Issued };

    public async Task<RatingResult> RatePreviewAsync(
        RatePreviewRequest request, Guid tenantId, CancellationToken ct = default)
    {
        var submission = await db.Submissions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId && s.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException($"Submission {request.SubmissionId} not found.");

        return await ratingEngine.RateAsync(submission, tenantId, ct);
    }

    public async Task<QuoteDetailDto?> RerateAsync(
        Guid id, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default)
    {
        var quote = await db.Quotes
            .FirstOrDefaultAsync(q => q.Id == id && q.TenantId == tenantId, ct);
        if (quote is null) return null;

        if (!RerateableStatuses.Contains(quote.Status))
            throw new InvalidOperationException(
                $"Cannot re-rate a quote with status '{quote.Status}'. " +
                "Only Draft or Issued quotes may be re-rated.");

        var submission = await db.Submissions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == quote.SubmissionId && s.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException($"Submission {quote.SubmissionId} not found.");

        var rating = await ratingEngine.RateAsync(submission, tenantId, ct);

        quote.TotalPremium = rating.TotalPremium;
        quote.Taxes = rating.Taxes;
        quote.Fees = rating.Fees;
        quote.TotalDue = rating.TotalDue;
        quote.Version += 1;
        quote.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Quote",
            EntityId: quote.Id,
            Action: MutationAction.Update,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return MapDetail(quote);
    }

    private static QuoteDetailDto MapDetail(Quote q) => new(
        q.Id, q.SubmissionId, q.Status, q.Version,
        q.TotalPremium, q.Taxes, q.Fees, q.TotalDue,
        q.EffectiveDate, q.ExpirationDate, q.QuoteExpiryDate,
        q.Coverages, q.Terms, q.BindRequestedAt,
        q.IssuedByUserId, q.CreatedAt, q.UpdatedAt);
}
