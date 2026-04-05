using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class SubmissionService(AppDbContext db, IMutationAuditService auditService) : ISubmissionService
{
    private static readonly IReadOnlyDictionary<SubmissionStatus, IReadOnlySet<SubmissionStatus>> AllowedTransitions =
        new Dictionary<SubmissionStatus, IReadOnlySet<SubmissionStatus>>
        {
            [SubmissionStatus.Draft]      = new HashSet<SubmissionStatus> { SubmissionStatus.Submitted, SubmissionStatus.Cancelled },
            [SubmissionStatus.Submitted]  = new HashSet<SubmissionStatus> { SubmissionStatus.InReview, SubmissionStatus.Cancelled },
            [SubmissionStatus.InReview]   = new HashSet<SubmissionStatus> { SubmissionStatus.Quoted, SubmissionStatus.Declined },
            [SubmissionStatus.Quoted]     = new HashSet<SubmissionStatus> { SubmissionStatus.Bound, SubmissionStatus.Cancelled },
            [SubmissionStatus.Bound]      = new HashSet<SubmissionStatus>(),
            [SubmissionStatus.Declined]   = new HashSet<SubmissionStatus>(),
            [SubmissionStatus.Cancelled]  = new HashSet<SubmissionStatus>()
        };

    public async Task<IReadOnlyList<SubmissionSummaryDto>> ListAsync(
        Guid tenantId, Guid requestingUserId, UserRole role, CancellationToken ct = default)
    {
        var query = db.Submissions.AsNoTracking().Where(s => s.TenantId == tenantId);

        if (role is UserRole.Agent or UserRole.Broker)
            query = query.Where(s => s.SubmittedByUserId == requestingUserId);
        else if (role == UserRole.Underwriter)
            query = query.Where(s => s.AssignedUnderwriterId == requestingUserId
                || s.Status == SubmissionStatus.Submitted); // Underwriters can also see unassigned submissions

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubmissionSummaryDto(
                s.Id, s.InsuredId, s.LineOfBusiness, s.Status,
                s.EffectiveDate, s.ExpirationDate,
                s.SubmittedByUserId, s.AssignedUnderwriterId, s.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<SubmissionDetailDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var s = await db.Submissions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return s is null ? null : MapDetail(s);
    }

    public async Task<SubmissionDetailDto> CreateAsync(
        CreateSubmissionRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var submission = new Submission
        {
            TenantId = tenantId,
            InsuredId = request.InsuredId,
            LineOfBusiness = request.LineOfBusiness,
            EffectiveDate = request.EffectiveDate,
            ExpirationDate = request.ExpirationDate,
            RiskData = request.RiskData,
            Notes = request.Notes,
            SubmittedByUserId = actorId,
            Status = SubmissionStatus.Draft
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Submission",
            EntityId: submission.Id,
            Action: MutationAction.Create,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return MapDetail(submission);
    }

    public async Task<SubmissionDetailDto?> UpdateAsync(
        Guid id, UpdateSubmissionRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var submission = await db.Submissions
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        if (submission is null) return null;

        if (request.AssignedUnderwriterId.HasValue)
            submission.AssignedUnderwriterId = request.AssignedUnderwriterId.Value;
        if (request.Notes is not null) submission.Notes = request.Notes;
        submission.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Submission",
            EntityId: submission.Id,
            Action: MutationAction.Update,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return MapDetail(submission);
    }

    public async Task<SubmissionDetailDto?> TransitionStatusAsync(
        Guid id, TransitionSubmissionStatusRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var submission = await db.Submissions
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, ct);
        if (submission is null) return null;

        if (!AllowedTransitions.TryGetValue(submission.Status, out var allowed) ||
            !allowed.Contains(request.NewStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition submission from {submission.Status} to {request.NewStatus}. " +
                $"Allowed: [{string.Join(", ", allowed ?? (IEnumerable<SubmissionStatus>)[])}]");
        }

        var oldStatus = submission.Status;
        submission.Status = request.NewStatus;
        submission.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.NewStatus == SubmissionStatus.Declined)
            submission.DeclineReason = request.DeclineReason;

        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Submission",
            EntityId: submission.Id,
            Action: MutationAction.StatusChange,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return MapDetail(submission);
    }

    private static SubmissionDetailDto MapDetail(Submission s) => new(
        s.Id, s.InsuredId, s.LineOfBusiness, s.Status,
        s.EffectiveDate, s.ExpirationDate, s.RiskData, s.Notes, s.DeclineReason,
        s.SubmittedByUserId, s.AssignedUnderwriterId, s.CreatedAt, s.UpdatedAt);
}
