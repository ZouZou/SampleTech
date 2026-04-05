using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class InsuredService(AppDbContext db, IMutationAuditService auditService) : IInsuredService
{
    public async Task<IReadOnlyList<InsuredSummaryDto>> ListAsync(
        Guid tenantId, Guid requestingUserId, UserRole role, CancellationToken ct = default)
    {
        var query = db.Insureds.AsNoTracking().Where(i => i.TenantId == tenantId);

        // Agents/brokers only see their assigned insureds
        if (role is UserRole.Agent or UserRole.Broker)
            query = query.Where(i => i.AssignedAgentId == requestingUserId);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InsuredSummaryDto(
                i.Id,
                i.Type,
                i.Type == InsuredType.Individual
                    ? (i.FirstName + " " + i.LastName).Trim()
                    : i.BusinessName ?? "",
                i.Email,
                i.Phone,
                i.AssignedAgentId,
                i.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<InsuredDetailDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var i = await db.Insureds.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
        return i is null ? null : MapDetail(i);
    }

    public async Task<InsuredDetailDto> CreateAsync(
        CreateInsuredRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var insured = new Insured
        {
            TenantId = tenantId,
            Type = request.Type,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            BusinessName = request.BusinessName,
            Phone = request.Phone,
            Address = request.Address ?? "{}",
            DateOfBirth = request.DateOfBirth,
            AssignedAgentId = request.AssignedAgentId
        };

        db.Insureds.Add(insured);
        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Insured",
            EntityId: insured.Id,
            Action: MutationAction.Create,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return MapDetail(insured);
    }

    public async Task<InsuredDetailDto?> UpdateAsync(
        Guid id, UpdateInsuredRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var insured = await db.Insureds
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId, ct);
        if (insured is null) return null;

        if (request.Email is not null) insured.Email = request.Email;
        if (request.Phone is not null) insured.Phone = request.Phone;
        if (request.Address is not null) insured.Address = request.Address;
        if (request.AssignedAgentId.HasValue) insured.AssignedAgentId = request.AssignedAgentId.Value;
        if (request.LinkedUserId.HasValue) insured.LinkedUserId = request.LinkedUserId.Value;
        insured.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Insured",
            EntityId: insured.Id,
            Action: MutationAction.Update,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString()), ct);

        return MapDetail(insured);
    }

    private static InsuredDetailDto MapDetail(Insured i) => new(
        i.Id, i.Type, i.FirstName, i.LastName, i.BusinessName,
        i.Email, i.Phone, i.Address, i.DateOfBirth,
        i.LinkedUserId, i.AssignedAgentId, i.CreatedAt, i.UpdatedAt);
}
