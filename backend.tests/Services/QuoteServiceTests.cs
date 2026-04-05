using FluentAssertions;
using SampleTech.Api.Models;
using SampleTech.Api.Services;
using SampleTech.Api.Tests.Helpers;
using Xunit;

namespace SampleTech.Api.Tests.Services;

public class QuoteServiceTests : IDisposable
{
    private readonly SampleTech.Api.Data.AppDbContext _db;
    private readonly QuoteService _sut;

    public QuoteServiceTests()
    {
        _db = TestDbContextFactory.Create();
        _sut = new QuoteService(_db, new MutationAuditService(_db), new RatingEngine(_db));
    }

    private (Guid tenantId, Guid uwId, Guid agentId, Guid submissionId) Seed()
    {
        var tenant = DomainTestHelpers.SeedTenant(_db);
        var uw = DomainTestHelpers.SeedUser(_db, tenant.Id, UserRole.Underwriter);
        var agent = DomainTestHelpers.SeedUser(_db, tenant.Id, UserRole.Agent);
        var insured = DomainTestHelpers.SeedInsured(_db, tenant.Id, agent.Id);
        var submission = DomainTestHelpers.SeedSubmission(_db, tenant.Id, insured.Id, agent.Id, SubmissionStatus.InReview);
        return (tenant.Id, uw.Id, agent.Id, submission.Id);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Create_ReturnsQuote_WithVersion1AndDraftStatus()
    {
        var (tenantId, uwId, _, submissionId) = Seed();

        var req = new CreateQuoteRequest(
            SubmissionId: submissionId,
            TotalPremium: 1200m,
            Taxes: 60m,
            Fees: 25m,
            QuoteExpiryDate: DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            Coverages: null,
            Terms: null);

        var result = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        result.Status.Should().Be(QuoteStatus.Draft);
        result.Version.Should().Be(1);
        result.TotalDue.Should().Be(1285m); // 1200 + 60 + 25
        result.IssuedByUserId.Should().Be(uwId);
    }

    [Fact]
    public async Task Create_SecondQuote_IncrementsVersion()
    {
        var (tenantId, uwId, _, submissionId) = Seed();

        var req = new CreateQuoteRequest(submissionId, 1200m, 60m, 25m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null, null);

        await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);
        var second = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        second.Version.Should().Be(2);
    }

    [Fact]
    public async Task Create_SupersedesPriorIssuedQuote()
    {
        var (tenantId, uwId, _, submissionId) = Seed();

        var req = new CreateQuoteRequest(submissionId, 1200m, 60m, 25m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null, null);

        var first = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        // Issue first quote
        await _sut.TransitionStatusAsync(first.Id,
            new TransitionQuoteStatusRequest(QuoteStatus.Issued),
            tenantId, uwId, UserRole.Underwriter);

        // Create a re-rated second quote — first should become Superseded
        await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        var firstFromDb = await _db.Quotes.FindAsync(first.Id);
        firstFromDb!.Status.Should().Be(QuoteStatus.Superseded);
    }

    // ── Lifecycle transitions ─────────────────────────────────────────────────
    [Fact]
    public async Task Transition_DraftToIssued_Succeeds()
    {
        var (tenantId, uwId, _, submissionId) = Seed();
        var req = new CreateQuoteRequest(submissionId, 1200m, 60m, 25m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null, null);
        var quote = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        var result = await _sut.TransitionStatusAsync(
            quote.Id,
            new TransitionQuoteStatusRequest(QuoteStatus.Issued),
            tenantId, uwId, UserRole.Underwriter);

        result!.Status.Should().Be(QuoteStatus.Issued);
    }

    [Fact]
    public async Task Transition_IssuedToAccepted_SetsBindRequestedAt()
    {
        var (tenantId, uwId, agentId, submissionId) = Seed();
        var req = new CreateQuoteRequest(submissionId, 1200m, 60m, 25m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null, null);
        var quote = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);
        await _sut.TransitionStatusAsync(quote.Id, new TransitionQuoteStatusRequest(QuoteStatus.Issued), tenantId, uwId, UserRole.Underwriter);

        var result = await _sut.TransitionStatusAsync(
            quote.Id,
            new TransitionQuoteStatusRequest(QuoteStatus.Accepted),
            tenantId, agentId, UserRole.Agent);

        result!.Status.Should().Be(QuoteStatus.Accepted);
        result.BindRequestedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(QuoteStatus.Draft, QuoteStatus.Accepted)]
    [InlineData(QuoteStatus.Draft, QuoteStatus.Declined)]
    [InlineData(QuoteStatus.Accepted, QuoteStatus.Issued)]
    [InlineData(QuoteStatus.Expired, QuoteStatus.Issued)]
    public async Task Transition_InvalidTransitions_Throw(QuoteStatus from, QuoteStatus to)
    {
        var (tenantId, uwId, _, submissionId) = Seed();
        var req = new CreateQuoteRequest(submissionId, 1200m, 60m, 25m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null, null);
        var quote = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        // Manually force status to `from`
        var dbQuote = await _db.Quotes.FindAsync(quote.Id);
        dbQuote!.Status = from;
        await _db.SaveChangesAsync();

        var act = async () => await _sut.TransitionStatusAsync(
            quote.Id, new TransitionQuoteStatusRequest(to), tenantId, uwId, UserRole.Underwriter);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── Tenant isolation ──────────────────────────────────────────────────────
    [Fact]
    public async Task GetById_ReturnsNull_WhenWrongTenant()
    {
        var (tenantId, uwId, _, submissionId) = Seed();
        var otherTenant = DomainTestHelpers.SeedTenant(_db, "other-co");

        var req = new CreateQuoteRequest(submissionId, 1200m, 60m, 25m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null, null);
        var quote = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        var result = await _sut.GetByIdAsync(quote.Id, otherTenant.Id);

        result.Should().BeNull();
    }

    // ── Audit ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Create_LogsAuditEntry()
    {
        var (tenantId, uwId, _, submissionId) = Seed();
        var req = new CreateQuoteRequest(submissionId, 1200m, 60m, 25m,
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null, null);

        var result = await _sut.CreateAsync(req, tenantId, uwId, UserRole.Underwriter);

        _db.MutationAuditLogs.Should().Contain(a =>
            a.EntityId == result.Id &&
            a.Action == MutationAction.Create &&
            a.EntityType == "Quote");
    }

    public void Dispose() => _db.Dispose();
}
