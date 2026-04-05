using FluentAssertions;
using SampleTech.Api.Models;
using SampleTech.Api.Services;
using SampleTech.Api.Tests.Helpers;
using Xunit;

namespace SampleTech.Api.Tests.Services;

public class PolicyServiceTests : IDisposable
{
    private readonly SampleTech.Api.Data.AppDbContext _db;
    private readonly MutationAuditService _auditSvc;
    private readonly PolicyService _sut;

    public PolicyServiceTests()
    {
        _db = TestDbContextFactory.Create();
        _auditSvc = new MutationAuditService(_db);
        _sut = new PolicyService(_db, _auditSvc, new NullDocumentStorageService());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private (Guid tenantId, Guid underwriterId, Guid insuredId) Seed()
    {
        var tenant = DomainTestHelpers.SeedTenant(_db);
        var uw = DomainTestHelpers.SeedUser(_db, tenant.Id, UserRole.Underwriter);
        var insured = DomainTestHelpers.SeedInsured(_db, tenant.Id);
        return (tenant.Id, uw.Id, insured.Id);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Create_ReturnsDraftPolicy_WithGeneratedPolicyNumber()
    {
        var (tenantId, uwId, insuredId) = Seed();

        var req = new CreatePolicyRequest(
            InsuredId: insuredId,
            LineOfBusiness: LineOfBusiness.Auto,
            EffectiveDate: DateOnly.FromDateTime(DateTime.Today),
            ExpirationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(365)),
            TotalPremium: 1200m,
            AssignedAgentId: null,
            QuoteId: null,
            Coverages: null);

        var result = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        result.Status.Should().Be(PolicyStatus.Draft);
        result.PolicyNumber.Should().NotBeNullOrWhiteSpace();
        result.TotalPremium.Should().Be(1200m);
        result.LineOfBusiness.Should().Be(LineOfBusiness.Auto);
    }

    [Fact]
    public async Task Create_WithCoverages_PersistsCoverages()
    {
        var (tenantId, uwId, insuredId) = Seed();

        var coverages = new List<CreateCoverageRequest>
        {
            new(CoverageType.BodilyInjury, "Bodily Injury $100k/$300k", 600m, 100_000m, 300_000m, 500m),
            new(CoverageType.Collision, "Collision", 400m, null, null, 1_000m)
        };

        var req = new CreatePolicyRequest(insuredId, LineOfBusiness.Auto,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(365)),
            1000m, null, null, coverages);

        var result = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        result.Coverages.Should().HaveCount(2);
        result.Coverages.Should().Contain(c => c.CoverageType == CoverageType.BodilyInjury);
    }

    [Fact]
    public async Task Create_LogsMutationAuditEntry()
    {
        var (tenantId, uwId, insuredId) = Seed();

        var req = new CreatePolicyRequest(insuredId, LineOfBusiness.Property,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(365)),
            2400m, null, null, null);

        var result = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        var audit = _db.MutationAuditLogs.SingleOrDefault(a =>
            a.EntityId == result.Id && a.Action == MutationAction.Create);
        audit.Should().NotBeNull();
        audit!.EntityType.Should().Be("Policy");
        audit.ActorUserId.Should().Be(uwId);
        audit.ActorRole.Should().Be("Underwriter");
    }

    // ── Tenant isolation ──────────────────────────────────────────────────────
    [Fact]
    public async Task List_DoesNotLeakAcrossTenants()
    {
        var tenant1 = DomainTestHelpers.SeedTenant(_db, "tenant-a");
        var tenant2 = DomainTestHelpers.SeedTenant(_db, "tenant-b");
        var uw1 = DomainTestHelpers.SeedUser(_db, tenant1.Id, UserRole.Underwriter);
        var uw2 = DomainTestHelpers.SeedUser(_db, tenant2.Id, UserRole.Underwriter);
        var ins1 = DomainTestHelpers.SeedInsured(_db, tenant1.Id);
        var ins2 = DomainTestHelpers.SeedInsured(_db, tenant2.Id);

        DomainTestHelpers.SeedPolicy(_db, tenant1.Id, ins1.Id, uw1.Id);
        DomainTestHelpers.SeedPolicy(_db, tenant2.Id, ins2.Id, uw2.Id);

        var tenant1Policies = await _sut.ListAsync(tenant1.Id, uw1.Id, UserRole.Underwriter);
        var tenant2Policies = await _sut.ListAsync(tenant2.Id, uw2.Id, UserRole.Underwriter);

        tenant1Policies.Should().HaveCount(1);
        tenant2Policies.Should().HaveCount(1);
        tenant1Policies[0].Id.Should().NotBe(tenant2Policies[0].Id);
    }

    [Fact]
    public async Task GetById_ReturnNull_WhenWrongTenant()
    {
        var tenant1 = DomainTestHelpers.SeedTenant(_db, "tenant-x");
        var tenant2 = DomainTestHelpers.SeedTenant(_db, "tenant-y");
        var uw1 = DomainTestHelpers.SeedUser(_db, tenant1.Id, UserRole.Underwriter);
        var uw2 = DomainTestHelpers.SeedUser(_db, tenant2.Id, UserRole.Underwriter);
        var ins1 = DomainTestHelpers.SeedInsured(_db, tenant1.Id);

        var policy = DomainTestHelpers.SeedPolicy(_db, tenant1.Id, ins1.Id, uw1.Id);

        // Tenant 2 trying to read Tenant 1's policy
        var result = await _sut.GetByIdAsync(policy.Id, tenant2.Id, uw2.Id, UserRole.Underwriter);

        result.Should().BeNull();
    }

    // ── State machine ─────────────────────────────────────────────────────────
    [Fact]
    public async Task Transition_DraftToQuoted_Succeeds()
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId, PolicyStatus.Draft);

        var result = await _sut.TransitionStatusAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(PolicyStatus.Quoted, null),
            tenantId, uwId, UserRole.Underwriter);

        result.Should().NotBeNull();
        result!.Status.Should().Be(PolicyStatus.Quoted);
    }

    [Fact]
    public async Task Transition_QuotedToBound_Succeeds()
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId, PolicyStatus.Quoted);

        var result = await _sut.TransitionStatusAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(PolicyStatus.Bound, null),
            tenantId, uwId, UserRole.Underwriter);

        result!.Status.Should().Be(PolicyStatus.Bound);
    }

    [Fact]
    public async Task Transition_BoundToActive_Succeeds()
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId, PolicyStatus.Bound);

        var result = await _sut.TransitionStatusAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(PolicyStatus.Active, null),
            tenantId, uwId, UserRole.Underwriter);

        result!.Status.Should().Be(PolicyStatus.Active);
    }

    [Fact]
    public async Task Transition_ActiveToExpired_Succeeds()
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId, PolicyStatus.Active);

        var result = await _sut.TransitionStatusAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(PolicyStatus.Expired, null),
            tenantId, uwId, UserRole.Underwriter);

        result!.Status.Should().Be(PolicyStatus.Expired);
    }

    [Fact]
    public async Task Transition_ActiveToCancelled_SetsCancellationFields()
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId, PolicyStatus.Active);

        var result = await _sut.TransitionStatusAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(PolicyStatus.Cancelled, "Non-payment"),
            tenantId, uwId, UserRole.Underwriter);

        result!.Status.Should().Be(PolicyStatus.Cancelled);
        result.CancellationReason.Should().Be("Non-payment");
        result.CancelledAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(PolicyStatus.Draft, PolicyStatus.Active)]
    [InlineData(PolicyStatus.Draft, PolicyStatus.Expired)]
    [InlineData(PolicyStatus.Draft, PolicyStatus.Bound)]
    [InlineData(PolicyStatus.Quoted, PolicyStatus.Active)]
    [InlineData(PolicyStatus.Quoted, PolicyStatus.Draft)]
    [InlineData(PolicyStatus.Active, PolicyStatus.Draft)]
    [InlineData(PolicyStatus.Active, PolicyStatus.Quoted)]
    [InlineData(PolicyStatus.Expired, PolicyStatus.Active)]
    [InlineData(PolicyStatus.Expired, PolicyStatus.Cancelled)]
    [InlineData(PolicyStatus.Cancelled, PolicyStatus.Active)]
    public async Task Transition_InvalidTransition_ThrowsInvalidOperationException(PolicyStatus from, PolicyStatus to)
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId, from);

        var act = async () => await _sut.TransitionStatusAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(to, null),
            tenantId, uwId, UserRole.Underwriter);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Transition_LogsStatusChangeAudit()
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId, PolicyStatus.Draft);

        await _sut.TransitionStatusAsync(
            policy.Id,
            new TransitionPolicyStatusRequest(PolicyStatus.Quoted, null),
            tenantId, uwId, UserRole.Underwriter);

        var audit = _db.MutationAuditLogs.SingleOrDefault(a =>
            a.EntityId == policy.Id && a.Action == MutationAction.StatusChange);
        audit.Should().NotBeNull();
        audit!.PreviousState.Should().Contain("Draft");
        audit.NextState.Should().Contain("Quoted");
    }

    // ── Update ────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Update_ModifiesFields_AndLogsAudit()
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId);
        var newExpiry = DateOnly.FromDateTime(DateTime.Today.AddDays(730));

        var result = await _sut.UpdateAsync(
            policy.Id,
            new UpdatePolicyRequest(newExpiry, 1500m, null),
            tenantId, uwId, UserRole.Underwriter);

        result!.ExpirationDate.Should().Be(newExpiry);
        result.TotalPremium.Should().Be(1500m);

        var audit = _db.MutationAuditLogs.SingleOrDefault(a =>
            a.EntityId == policy.Id && a.Action == MutationAction.Update);
        audit.Should().NotBeNull();
    }

    // ── Document attachment ───────────────────────────────────────────────────
    [Fact]
    public async Task AttachDocument_StoresDocumentAndReturnsDto()
    {
        var (tenantId, uwId, insuredId) = Seed();
        var policy = DomainTestHelpers.SeedPolicy(_db, tenantId, insuredId, uwId);

        var stream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
        var doc = await _sut.AttachDocumentAsync(
            policy.Id, "endorsement.pdf", "application/pdf", stream, stream.Length, tenantId, uwId);

        doc.Should().NotBeNull();
        doc!.FileName.Should().Be("endorsement.pdf");
        doc.BlobUrl.Should().Contain(policy.Id.ToString());
    }

    [Fact]
    public async Task GetDocuments_ReturnsOnlyDocumentsForTenant()
    {
        var tenant1 = DomainTestHelpers.SeedTenant(_db, "doc-tenant-a");
        var tenant2 = DomainTestHelpers.SeedTenant(_db, "doc-tenant-b");
        var uw1 = DomainTestHelpers.SeedUser(_db, tenant1.Id, UserRole.Underwriter);
        var uw2 = DomainTestHelpers.SeedUser(_db, tenant2.Id, UserRole.Underwriter);
        var ins1 = DomainTestHelpers.SeedInsured(_db, tenant1.Id);
        var policy = DomainTestHelpers.SeedPolicy(_db, tenant1.Id, ins1.Id, uw1.Id);

        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        await _sut.AttachDocumentAsync(policy.Id, "doc.pdf", "application/pdf", stream, 3, tenant1.Id, uw1.Id);

        // Tenant 2 requesting documents for tenant 1's policy — should return empty (policy not found in tenant2)
        var docs = await _sut.GetDocumentsAsync(policy.Id, tenant2.Id);
        docs.Should().BeEmpty();
    }

    public void Dispose() => _db.Dispose();
}
