using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class ClaimService(AppDbContext db) : IClaimService
{
    public async Task<IReadOnlyList<ClaimSummaryDto>> ListAsync(
        Guid requestingUserId, UserRole role, CancellationToken ct = default)
    {
        var query = db.Claims.AsNoTracking();

        if (role == UserRole.Client)
            query = query.Where(c => c.ClaimantId == requestingUserId);
        else if (role is UserRole.Agent or UserRole.Broker)
            query = query.Where(c => c.Policy.AssignedAgentId == requestingUserId);

        return await query
            .OrderByDescending(c => c.SubmittedAt)
            .Select(c => new ClaimSummaryDto(
                c.Id, c.ClaimNumber, c.Status, c.ClaimedAmount, c.ApprovedAmount,
                c.IncidentDate, c.SubmittedAt, c.PolicyId, c.ClaimantId))
            .ToListAsync(ct);
    }

    public async Task<ClaimDetailDto?> GetByIdAsync(
        Guid id, Guid requestingUserId, UserRole role, CancellationToken ct = default)
    {
        var c = await db.Claims.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (c is null) return null;

        if (role == UserRole.Client && c.ClaimantId != requestingUserId)
            return null;

        return MapDetail(c);
    }

    public async Task<ClaimDetailDto> FileAsync(
        FileClaim request, Guid claimantId, CancellationToken ct = default)
    {
        var claim = new Claim
        {
            ClaimNumber = GenerateClaimNumber(),
            PolicyId = request.PolicyId,
            ClaimantId = claimantId,
            Description = request.Description,
            ClaimedAmount = request.ClaimedAmount,
            IncidentDate = request.IncidentDate,
            Status = ClaimStatus.Submitted
        };

        db.Claims.Add(claim);
        await db.SaveChangesAsync(ct);
        return MapDetail(claim);
    }

    public async Task<ClaimDetailDto?> UpdateAsync(
        Guid id, UpdateClaimRequest request, Guid reviewerId, CancellationToken ct = default)
    {
        var claim = await db.Claims.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (claim is null) return null;

        if (request.Status.HasValue) claim.Status = request.Status.Value;
        if (request.ApprovedAmount.HasValue) claim.ApprovedAmount = request.ApprovedAmount.Value;
        if (request.ReviewNotes is not null) claim.ReviewNotes = request.ReviewNotes;
        claim.ReviewedByUserId = reviewerId;
        claim.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return MapDetail(claim);
    }

    private static ClaimDetailDto MapDetail(Claim c) => new(
        c.Id, c.ClaimNumber, c.Status, c.Description, c.ClaimedAmount, c.ApprovedAmount,
        c.IncidentDate, c.SubmittedAt, c.UpdatedAt, c.PolicyId, c.ClaimantId,
        c.ReviewedByUserId, c.ReviewNotes);

    private static string GenerateClaimNumber() =>
        $"CLM-{DateTimeOffset.UtcNow:yyyyMM}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
