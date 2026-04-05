using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record InsuredSummaryDto(
    Guid Id,
    InsuredType Type,
    string DisplayName,
    string Email,
    string? Phone,
    Guid? AssignedAgentId,
    DateTimeOffset CreatedAt);

public record InsuredDetailDto(
    Guid Id,
    InsuredType Type,
    string? FirstName,
    string? LastName,
    string? BusinessName,
    string Email,
    string? Phone,
    string Address,
    DateOnly? DateOfBirth,
    Guid? LinkedUserId,
    Guid? AssignedAgentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateInsuredRequest(
    InsuredType Type,
    string Email,
    string? FirstName,
    string? LastName,
    string? BusinessName,
    string? Phone,
    string? Address,
    DateOnly? DateOfBirth,
    Guid? AssignedAgentId);

public record UpdateInsuredRequest(
    string? Email,
    string? Phone,
    string? Address,
    Guid? AssignedAgentId,
    Guid? LinkedUserId);

public interface IInsuredService
{
    Task<IReadOnlyList<InsuredSummaryDto>> ListAsync(Guid tenantId, Guid requestingUserId, UserRole role, CancellationToken ct = default);
    Task<InsuredDetailDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<InsuredDetailDto> CreateAsync(CreateInsuredRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
    Task<InsuredDetailDto?> UpdateAsync(Guid id, UpdateInsuredRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
}
