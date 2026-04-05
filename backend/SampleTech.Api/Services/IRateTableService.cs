using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record RateTableDto(
    Guid Id,
    LineOfBusiness LineOfBusiness,
    string ProductCode,
    int TableVersion,
    bool IsActive,
    decimal BaseRate,
    decimal TaxRate,
    decimal FlatFee,
    string FactorsJson,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CreateRateTableRequest(
    LineOfBusiness LineOfBusiness,
    string ProductCode,
    decimal BaseRate,
    decimal TaxRate,
    decimal FlatFee,
    string? FactorsJson,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo);

public record UpdateRateTableRequest(
    decimal BaseRate,
    decimal TaxRate,
    decimal FlatFee,
    string? FactorsJson,
    bool IsActive,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo);

public interface IRateTableService
{
    Task<IReadOnlyList<RateTableDto>> ListAsync(Guid tenantId, LineOfBusiness? lob = null, CancellationToken ct = default);
    Task<RateTableDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<RateTableDto> CreateAsync(CreateRateTableRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
    Task<RateTableDto?> UpdateAsync(Guid id, UpdateRateTableRequest request, Guid tenantId, Guid actorId, UserRole actorRole, CancellationToken ct = default);
}
