using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class RateTableService(AppDbContext db, IMutationAuditService auditService) : IRateTableService
{
    public async Task<IReadOnlyList<RateTableDto>> ListAsync(
        Guid tenantId, LineOfBusiness? lob = null, CancellationToken ct = default)
    {
        var query = db.RateTables.AsNoTracking()
            .Where(rt => rt.TenantId == tenantId);

        if (lob.HasValue)
            query = query.Where(rt => rt.LineOfBusiness == lob.Value);

        return await query
            .OrderBy(rt => rt.LineOfBusiness)
            .ThenByDescending(rt => rt.TableVersion)
            .Select(rt => Map(rt))
            .ToListAsync(ct);
    }

    public async Task<RateTableDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var rt = await db.RateTables.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return rt is null ? null : Map(rt);
    }

    public async Task<RateTableDto> CreateAsync(
        CreateRateTableRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var maxVersion = await db.RateTables.AsNoTracking()
            .Where(rt => rt.TenantId == tenantId
                      && rt.LineOfBusiness == request.LineOfBusiness
                      && rt.ProductCode == request.ProductCode)
            .MaxAsync(rt => (int?)rt.TableVersion, ct) ?? 0;

        var rt = new RateTable
        {
            TenantId = tenantId,
            LineOfBusiness = request.LineOfBusiness,
            ProductCode = request.ProductCode,
            TableVersion = maxVersion + 1,
            IsActive = true,
            BaseRate = request.BaseRate,
            TaxRate = request.TaxRate,
            FlatFee = request.FlatFee,
            FactorsJson = request.FactorsJson ?? "[]",
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };

        db.RateTables.Add(rt);
        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "RateTable",
            EntityId: rt.Id,
            Action: MutationAction.Create,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return Map(rt);
    }

    public async Task<RateTableDto?> UpdateAsync(
        Guid id, UpdateRateTableRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var rt = await db.RateTables
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        if (rt is null) return null;

        rt.BaseRate = request.BaseRate;
        rt.TaxRate = request.TaxRate;
        rt.FlatFee = request.FlatFee;
        rt.FactorsJson = request.FactorsJson ?? "[]";
        rt.IsActive = request.IsActive;
        rt.EffectiveFrom = request.EffectiveFrom;
        rt.EffectiveTo = request.EffectiveTo;
        rt.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "RateTable",
            EntityId: rt.Id,
            Action: MutationAction.Update,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return Map(rt);
    }

    private static RateTableDto Map(RateTable rt) => new(
        rt.Id, rt.LineOfBusiness, rt.ProductCode, rt.TableVersion, rt.IsActive,
        rt.BaseRate, rt.TaxRate, rt.FlatFee, rt.FactorsJson,
        rt.EffectiveFrom, rt.EffectiveTo, rt.CreatedAt, rt.UpdatedAt);
}
