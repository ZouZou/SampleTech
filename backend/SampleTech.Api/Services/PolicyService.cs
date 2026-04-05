using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class PolicyService(
    AppDbContext db,
    IMutationAuditService auditService,
    IDocumentStorageService documentStorage) : IPolicyService
{
    // ── Valid state machine transitions ───────────────────────────────────────
    private static readonly IReadOnlyDictionary<PolicyStatus, IReadOnlySet<PolicyStatus>> AllowedTransitions =
        new Dictionary<PolicyStatus, IReadOnlySet<PolicyStatus>>
        {
            [PolicyStatus.Draft]    = new HashSet<PolicyStatus> { PolicyStatus.Quoted, PolicyStatus.Cancelled },
            [PolicyStatus.Quoted]   = new HashSet<PolicyStatus> { PolicyStatus.Bound, PolicyStatus.Cancelled },
            [PolicyStatus.Bound]    = new HashSet<PolicyStatus> { PolicyStatus.Active, PolicyStatus.Cancelled },
            [PolicyStatus.Active]   = new HashSet<PolicyStatus> { PolicyStatus.Expired, PolicyStatus.Cancelled },
            [PolicyStatus.Expired]  = new HashSet<PolicyStatus>(),
            [PolicyStatus.Cancelled] = new HashSet<PolicyStatus>()
        };

    // ── List ──────────────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<PolicySummaryDto>> ListAsync(
        Guid tenantId, Guid requestingUserId, UserRole role, CancellationToken ct = default)
    {
        var query = db.Policies.AsNoTracking().Where(p => p.TenantId == tenantId);

        if (role is UserRole.Agent or UserRole.Broker)
            query = query.Where(p => p.AssignedAgentId == requestingUserId);
        else if (role == UserRole.Client)
            query = query.Where(p => p.Insured.LinkedUserId == requestingUserId);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PolicySummaryDto(
                p.Id, p.PolicyNumber, p.LineOfBusiness, p.Status,
                p.TotalPremium, p.EffectiveDate, p.ExpirationDate,
                p.InsuredId, p.AssignedAgentId, p.CreatedAt))
            .ToListAsync(ct);
    }

    // ── Get by ID ─────────────────────────────────────────────────────────────
    public async Task<PolicyDetailDto?> GetByIdAsync(
        Guid id, Guid tenantId, Guid requestingUserId, UserRole role, CancellationToken ct = default)
    {
        var policy = await db.Policies
            .AsNoTracking()
            .Include(p => p.Coverages)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);

        if (policy is null) return null;

        if (role == UserRole.Client)
        {
            var insured = await db.Insureds
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == policy.InsuredId && i.LinkedUserId == requestingUserId, ct);
            if (insured is null) return null;
        }

        return MapDetail(policy);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    public async Task<PolicyDetailDto> CreateAsync(
        CreatePolicyRequest request, Guid tenantId, Guid underwriterId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var tenantCode = await GetTenantCodeAsync(tenantId, ct);

        var policy = new Policy
        {
            TenantId = tenantId,
            InsuredId = request.InsuredId,
            QuoteId = request.QuoteId,
            LineOfBusiness = request.LineOfBusiness,
            EffectiveDate = request.EffectiveDate,
            ExpirationDate = request.ExpirationDate,
            TotalPremium = request.TotalPremium,
            IssuedByUserId = underwriterId,
            AssignedAgentId = request.AssignedAgentId,
            PolicyNumber = GeneratePolicyNumber(tenantCode, request.LineOfBusiness),
            Status = PolicyStatus.Draft
        };

        if (request.Coverages is { Count: > 0 })
        {
            foreach (var c in request.Coverages)
            {
                policy.Coverages.Add(new Coverage
                {
                    TenantId = tenantId,
                    CoverageType = c.CoverageType,
                    Description = c.Description,
                    Premium = c.Premium,
                    LimitPerOccurrence = c.LimitPerOccurrence,
                    LimitAggregate = c.LimitAggregate,
                    Deductible = c.Deductible,
                    EffectiveDate = request.EffectiveDate
                });
            }
        }

        db.Policies.Add(policy);
        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Policy",
            EntityId: policy.Id,
            Action: MutationAction.Create,
            ActorUserId: underwriterId,
            ActorRole: actorRole.ToString(),
            NextState: SerializePolicy(policy)), ct);

        return MapDetail(policy);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    public async Task<PolicyDetailDto?> UpdateAsync(
        Guid id, UpdatePolicyRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var policy = await db.Policies
            .Include(p => p.Coverages)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);
        if (policy is null) return null;

        var before = SerializePolicy(policy);

        if (request.ExpirationDate.HasValue) policy.ExpirationDate = request.ExpirationDate.Value;
        if (request.TotalPremium.HasValue) policy.TotalPremium = request.TotalPremium.Value;
        if (request.AssignedAgentId.HasValue) policy.AssignedAgentId = request.AssignedAgentId.Value;
        policy.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Policy",
            EntityId: policy.Id,
            Action: MutationAction.Update,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString(),
            PreviousState: before,
            NextState: SerializePolicy(policy)), ct);

        return MapDetail(policy);
    }

    // ── State transition ──────────────────────────────────────────────────────
    public async Task<PolicyDetailDto?> TransitionStatusAsync(
        Guid id, TransitionPolicyStatusRequest request, Guid tenantId, Guid actorId, UserRole actorRole,
        CancellationToken ct = default)
    {
        var policy = await db.Policies
            .Include(p => p.Coverages)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);
        if (policy is null) return null;

        if (!AllowedTransitions.TryGetValue(policy.Status, out var allowed) ||
            !allowed.Contains(request.NewStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition policy from {policy.Status} to {request.NewStatus}. " +
                $"Allowed: [{string.Join(", ", allowed ?? (IEnumerable<PolicyStatus>)[])}]");
        }

        var before = SerializePolicy(policy);
        var oldStatus = policy.Status;

        policy.Status = request.NewStatus;
        policy.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.NewStatus == PolicyStatus.Cancelled)
        {
            policy.CancelledAt = DateTimeOffset.UtcNow;
            policy.CancellationReason = request.CancellationReason;
        }

        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new MutationAuditContext(
            TenantId: tenantId,
            EntityType: "Policy",
            EntityId: policy.Id,
            Action: MutationAction.StatusChange,
            ActorUserId: actorId,
            ActorRole: actorRole.ToString(),
            PreviousState: before,
            NextState: SerializePolicy(policy)), ct);

        return MapDetail(policy);
    }

    // ── Document attachment ───────────────────────────────────────────────────
    public async Task<PolicyDocumentDto?> AttachDocumentAsync(
        Guid policyId, string fileName, string contentType, Stream fileStream, long fileSize,
        Guid tenantId, Guid actorId, CancellationToken ct = default)
    {
        var policy = await db.Policies
            .FirstOrDefaultAsync(p => p.Id == policyId && p.TenantId == tenantId, ct);
        if (policy is null) return null;

        var blobUrl = await documentStorage.StoreAsync(tenantId, policyId, fileName, contentType, fileStream, ct);

        var doc = new PolicyDocument
        {
            PolicyId = policyId,
            TenantId = tenantId,
            UploadedByUserId = actorId,
            FileName = fileName,
            ContentType = contentType,
            BlobUrl = blobUrl,
            FileSizeBytes = fileSize
        };

        db.PolicyDocuments.Add(doc);
        await db.SaveChangesAsync(ct);

        return MapDocument(doc);
    }

    // ── Get documents ─────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<PolicyDocumentDto>> GetDocumentsAsync(
        Guid policyId, Guid tenantId, CancellationToken ct = default)
    {
        return await db.PolicyDocuments
            .AsNoTracking()
            .Where(d => d.PolicyId == policyId && d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => MapDocument(d))
            .ToListAsync(ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static PolicyDetailDto MapDetail(Policy p) => new(
        p.Id, p.PolicyNumber, p.LineOfBusiness, p.Status,
        p.TotalPremium, p.EffectiveDate, p.ExpirationDate,
        p.InsuredId, p.IssuedByUserId, p.AssignedAgentId, p.QuoteId,
        p.CancelledAt, p.CancellationReason, p.RenewalPolicyId,
        p.Coverages.Select(c => new CoverageSummaryDto(
            c.Id, c.CoverageType, c.Description,
            c.LimitPerOccurrence, c.LimitAggregate, c.Deductible,
            c.Premium, c.IsActive, c.EffectiveDate)).ToList(),
        p.Documents.Select(MapDocument).ToList(),
        p.CreatedAt, p.UpdatedAt);

    private static PolicyDocumentDto MapDocument(PolicyDocument d) => new(
        d.Id, d.FileName, d.ContentType, d.BlobUrl,
        d.FileSizeBytes, d.UploadedByUserId, d.CreatedAt);

    private static readonly JsonSerializerOptions _auditSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static string SerializePolicy(Policy p) =>
        JsonSerializer.Serialize(new
        {
            p.PolicyNumber,
            Status = p.Status.ToString(),
            LineOfBusiness = p.LineOfBusiness.ToString(),
            p.TotalPremium, p.EffectiveDate, p.ExpirationDate,
            p.InsuredId, p.IssuedByUserId, p.AssignedAgentId,
            p.CancelledAt, p.CancellationReason
        });

    private async Task<string> GetTenantCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var slug = await db.Tenants.AsNoTracking()
            .Where(t => t.Id == tenantId)
            .Select(t => t.Slug)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(slug)) return "GEN";
        return slug.Replace("-", "").ToUpperInvariant()[..Math.Min(5, slug.Replace("-", "").Length)];
    }

    private static string LobCode(LineOfBusiness lob) => lob switch
    {
        LineOfBusiness.Auto             => "AUTO",
        LineOfBusiness.Property         => "PROP",
        LineOfBusiness.GeneralLiability => "GL",
        LineOfBusiness.WorkersComp      => "WC",
        LineOfBusiness.Umbrella         => "UMB",
        _                               => "OTH"
    };

    private static string GeneratePolicyNumber(string tenantCode, LineOfBusiness lob) =>
        $"{tenantCode}-{LobCode(lob)}-{DateTimeOffset.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..5].ToUpperInvariant()}";
}
