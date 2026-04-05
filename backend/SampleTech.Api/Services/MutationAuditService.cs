using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class MutationAuditService(AppDbContext db) : IMutationAuditService
{
    public async Task LogAsync(MutationAuditContext ctx, CancellationToken ct = default)
    {
        db.MutationAuditLogs.Add(new MutationAuditLog
        {
            TenantId = ctx.TenantId,
            EntityType = ctx.EntityType,
            EntityId = ctx.EntityId,
            Action = ctx.Action,
            ActorUserId = ctx.ActorUserId,
            ActorRole = ctx.ActorRole,
            PreviousState = ctx.PreviousState,
            NextState = ctx.NextState,
            IpAddress = ctx.IpAddress,
            UserAgent = ctx.UserAgent
        });

        await db.SaveChangesAsync(ct);
    }
}
