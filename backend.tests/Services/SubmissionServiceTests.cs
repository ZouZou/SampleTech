using FluentAssertions;
using SampleTech.Api.Models;
using SampleTech.Api.Services;
using SampleTech.Api.Tests.Helpers;
using Xunit;

namespace SampleTech.Api.Tests.Services;

public class SubmissionServiceTests : IDisposable
{
    private readonly SampleTech.Api.Data.AppDbContext _db;
    private readonly SubmissionService _sut;

    public SubmissionServiceTests()
    {
        _db = TestDbContextFactory.Create();
        _sut = new SubmissionService(_db, new MutationAuditService(_db));
    }

    private (Guid tenantId, Guid agentId, Guid underwriterId, Guid insuredId) Seed()
    {
        var tenant = DomainTestHelpers.SeedTenant(_db);
        var agent = DomainTestHelpers.SeedUser(_db, tenant.Id, UserRole.Agent);
        var uw = DomainTestHelpers.SeedUser(_db, tenant.Id, UserRole.Underwriter);
        var insured = DomainTestHelpers.SeedInsured(_db, tenant.Id, agent.Id);
        return (tenant.Id, agent.Id, uw.Id, insured.Id);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Create_ReturnsDraftSubmission()
    {
        var (tenantId, agentId, _, insuredId) = Seed();

        var req = new CreateSubmissionRequest(
            InsuredId: insuredId,
            LineOfBusiness: LineOfBusiness.Auto,
            EffectiveDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
            RiskData: @"{""vehicleYear"":2022}",
            Notes: "Rush submission");

        var result = await _sut.CreateAsync(req, tenantId, agentId, UserRole.Agent);

        result.Status.Should().Be(SubmissionStatus.Draft);
        result.SubmittedByUserId.Should().Be(agentId);
        result.LineOfBusiness.Should().Be(LineOfBusiness.Auto);
    }

    // ── Lifecycle transitions ─────────────────────────────────────────────────
    [Fact]
    public async Task Transition_DraftToSubmitted_Succeeds()
    {
        var (tenantId, agentId, _, insuredId) = Seed();
        var submission = DomainTestHelpers.SeedSubmission(_db, tenantId, insuredId, agentId, SubmissionStatus.Draft);

        var result = await _sut.TransitionStatusAsync(
            submission.Id,
            new TransitionSubmissionStatusRequest(SubmissionStatus.Submitted, null),
            tenantId, agentId, UserRole.Agent);

        result!.Status.Should().Be(SubmissionStatus.Submitted);
    }

    [Fact]
    public async Task Transition_SubmittedToInReview_Succeeds()
    {
        var (tenantId, agentId, uwId, insuredId) = Seed();
        var submission = DomainTestHelpers.SeedSubmission(_db, tenantId, insuredId, agentId, SubmissionStatus.Submitted);

        var result = await _sut.TransitionStatusAsync(
            submission.Id,
            new TransitionSubmissionStatusRequest(SubmissionStatus.InReview, null),
            tenantId, uwId, UserRole.Underwriter);

        result!.Status.Should().Be(SubmissionStatus.InReview);
    }

    [Fact]
    public async Task Transition_InReviewToDeclined_SetsDeclineReason()
    {
        var (tenantId, agentId, uwId, insuredId) = Seed();
        var submission = DomainTestHelpers.SeedSubmission(_db, tenantId, insuredId, agentId, SubmissionStatus.InReview);

        var result = await _sut.TransitionStatusAsync(
            submission.Id,
            new TransitionSubmissionStatusRequest(SubmissionStatus.Declined, "High risk profile"),
            tenantId, uwId, UserRole.Underwriter);

        result!.Status.Should().Be(SubmissionStatus.Declined);
        result.DeclineReason.Should().Be("High risk profile");
    }

    [Theory]
    [InlineData(SubmissionStatus.Draft, SubmissionStatus.InReview)]
    [InlineData(SubmissionStatus.Draft, SubmissionStatus.Bound)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Quoted)]
    [InlineData(SubmissionStatus.Declined, SubmissionStatus.Submitted)]
    [InlineData(SubmissionStatus.Bound, SubmissionStatus.Submitted)]
    public async Task Transition_InvalidTransitions_Throw(SubmissionStatus from, SubmissionStatus to)
    {
        var (tenantId, agentId, uwId, insuredId) = Seed();
        var submission = DomainTestHelpers.SeedSubmission(_db, tenantId, insuredId, agentId, from);

        var act = async () => await _sut.TransitionStatusAsync(
            submission.Id,
            new TransitionSubmissionStatusRequest(to, null),
            tenantId, uwId, UserRole.Underwriter);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── Tenant isolation ──────────────────────────────────────────────────────
    [Fact]
    public async Task GetById_ReturnsNull_WhenWrongTenant()
    {
        var (tenantId, agentId, _, insuredId) = Seed();
        var otherTenant = DomainTestHelpers.SeedTenant(_db, "other-tenant");
        var submission = DomainTestHelpers.SeedSubmission(_db, tenantId, insuredId, agentId);

        var result = await _sut.GetByIdAsync(submission.Id, otherTenant.Id);

        result.Should().BeNull();
    }

    // ── Audit trail ───────────────────────────────────────────────────────────
    [Fact]
    public async Task Create_LogsAuditEntry()
    {
        var (tenantId, agentId, _, insuredId) = Seed();

        var req = new CreateSubmissionRequest(insuredId, LineOfBusiness.Property,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
            "{}", null);

        var result = await _sut.CreateAsync(req, tenantId, agentId, UserRole.Agent);

        _db.MutationAuditLogs.Should().Contain(a =>
            a.EntityId == result.Id &&
            a.Action == MutationAction.Create &&
            a.EntityType == "Submission");
    }

    public void Dispose() => _db.Dispose();
}
