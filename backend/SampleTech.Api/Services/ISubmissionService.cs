using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record SubmissionSummaryDto(
    Guid Id,
    Guid InsuredId,
    LineOfBusiness LineOfBusiness,
    SubmissionStatus Status,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    Guid SubmittedByUserId,
    Guid? AssignedUnderwriterId,
    DateTimeOffset CreatedAt);

public record SubmissionDetailDto(
    Guid Id,
    Guid InsuredId,
    LineOfBusiness LineOfBusiness,
    SubmissionStatus Status,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    string RiskData,
    string? Notes,
    string? DeclineReason,
    Guid SubmittedByUserId,
    Guid? AssignedUnderwriterId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateSubmissionRequest(
    Guid InsuredId,
    LineOfBusiness LineOfBusiness,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    string RiskData,
    string? Notes);

public record UpdateSubmissionRequest(
    Guid? AssignedUnderwriterId,
    string? Notes);

public record TransitionSubmissionStatusRequest(
    SubmissionStatus NewStatus,
    string? DeclineReason);

public interface ISubmissionService
{
    Task<IReadOnlyList<SubmissionSummaryDto>> ListAsync(Guid tenantId, Guid requestingUserId, UserRole role, CancellationToken ct = default);
    Task<SubmissionDetailDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<SubmissionDetailDto> CreateAsync(CreateSubmissionRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
    Task<SubmissionDetailDto?> UpdateAsync(Guid id, UpdateSubmissionRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
    Task<SubmissionDetailDto?> TransitionStatusAsync(Guid id, TransitionSubmissionStatusRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
}
