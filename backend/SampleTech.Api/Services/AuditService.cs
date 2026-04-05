using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class AuditService(AppDbContext db) : IAuditService
{
    public async Task LogAsync(AuditEventContext ctx, CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            EventType = ctx.EventType,
            Email = ctx.Email,
            UserId = ctx.UserId,
            TenantId = ctx.TenantId,
            IpAddress = ctx.IpAddress,
            UserAgent = ctx.UserAgent,
            Metadata = ctx.Metadata
        });

        await db.SaveChangesAsync(ct);
    }
}
