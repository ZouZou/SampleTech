using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record ClaimSummaryDto(
    Guid Id,
    string ClaimNumber,
    ClaimStatus Status,
    decimal ClaimedAmount,
    decimal? ApprovedAmount,
    DateOnly IncidentDate,
    DateTimeOffset SubmittedAt,
    Guid PolicyId,
    Guid ClaimantId);

public record ClaimDetailDto(
    Guid Id,
    string ClaimNumber,
    ClaimStatus Status,
    string Description,
    decimal ClaimedAmount,
    decimal? ApprovedAmount,
    DateOnly IncidentDate,
    DateTimeOffset SubmittedAt,
    DateTimeOffset UpdatedAt,
    Guid PolicyId,
    Guid ClaimantId,
    Guid? ReviewedByUserId,
    string? ReviewNotes);

public record FileClaim(
    Guid PolicyId,
    string Description,
    decimal ClaimedAmount,
    DateOnly IncidentDate);

public record UpdateClaimRequest(
    ClaimStatus? Status,
    decimal? ApprovedAmount,
    string? ReviewNotes);

public interface IClaimService
{
    Task<IReadOnlyList<ClaimSummaryDto>> ListAsync(Guid requestingUserId, UserRole role, CancellationToken ct = default);
    Task<ClaimDetailDto?> GetByIdAsync(Guid id, Guid requestingUserId, UserRole role, CancellationToken ct = default);
    Task<ClaimDetailDto> FileAsync(FileClaim request, Guid claimantId, CancellationToken ct = default);
    Task<ClaimDetailDto?> UpdateAsync(Guid id, UpdateClaimRequest request, Guid reviewerId, CancellationToken ct = default);
}
