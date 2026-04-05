using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record PolicySummaryDto(
    Guid Id,
    string PolicyNumber,
    LineOfBusiness LineOfBusiness,
    PolicyStatus Status,
    decimal TotalPremium,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    Guid InsuredId,
    Guid? AssignedAgentId,
    DateTimeOffset CreatedAt);

public record PolicyDetailDto(
    Guid Id,
    string PolicyNumber,
    LineOfBusiness LineOfBusiness,
    PolicyStatus Status,
    decimal TotalPremium,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    Guid InsuredId,
    Guid IssuedByUserId,
    Guid? AssignedAgentId,
    Guid? QuoteId,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    Guid? RenewalPolicyId,
    IReadOnlyList<CoverageSummaryDto> Coverages,
    IReadOnlyList<PolicyDocumentDto> Documents,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CoverageSummaryDto(
    Guid Id,
    CoverageType CoverageType,
    string Description,
    decimal? LimitPerOccurrence,
    decimal? LimitAggregate,
    decimal? Deductible,
    decimal Premium,
    bool IsActive,
    DateOnly EffectiveDate);

public record PolicyDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    string BlobUrl,
    long FileSizeBytes,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);

public record CreatePolicyRequest(
    Guid InsuredId,
    LineOfBusiness LineOfBusiness,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    decimal TotalPremium,
    Guid? AssignedAgentId,
    Guid? QuoteId,
    List<CreateCoverageRequest>? Coverages);

public record CreateCoverageRequest(
    CoverageType CoverageType,
    string Description,
    decimal Premium,
    decimal? LimitPerOccurrence,
    decimal? LimitAggregate,
    decimal? Deductible);

public record UpdatePolicyRequest(
    DateOnly? ExpirationDate,
    decimal? TotalPremium,
    Guid? AssignedAgentId);

public record TransitionPolicyStatusRequest(
    PolicyStatus NewStatus,
    string? CancellationReason);

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IPolicyService
{
    Task<IReadOnlyList<PolicySummaryDto>> ListAsync(Guid tenantId, Guid requestingUserId, UserRole role, CancellationToken ct = default);
    Task<PolicyDetailDto?> GetByIdAsync(Guid id, Guid tenantId, Guid requestingUserId, UserRole role, CancellationToken ct = default);
    Task<PolicyDetailDto> CreateAsync(CreatePolicyRequest request, Guid tenantId, Guid underwriterId, UserRole actorRole, CancellationToken ct = default);
    Task<PolicyDetailDto?> UpdateAsync(Guid id, UpdatePolicyRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
    Task<PolicyDetailDto?> TransitionStatusAsync(Guid id, TransitionPolicyStatusRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
    Task<PolicyDocumentDto?> AttachDocumentAsync(Guid policyId, string fileName, string contentType, Stream fileStream, long fileSize, Guid tenantId, Guid actorId, CancellationToken ct = default);
    Task<IReadOnlyList<PolicyDocumentDto>> GetDocumentsAsync(Guid policyId, Guid tenantId, CancellationToken ct = default);
}
