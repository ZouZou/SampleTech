using FluentAssertions;
using SampleTech.Api.Models;
using SampleTech.Api.Services;
using SampleTech.Api.Tests.Helpers;
using System.Text.Json;
using Xunit;

namespace SampleTech.Api.Tests.Services;

public class RatingEngineTests : IDisposable
{
    private readonly SampleTech.Api.Data.AppDbContext _db;
    private readonly RatingEngine _sut;

    public RatingEngineTests()
    {
        _db = TestDbContextFactory.Create();
        _sut = new RatingEngine(_db);
    }

    private (Guid tenantId, Submission submission) SeedAutoSubmission(
        string riskDataJson = @"{""vehicleYear"":2022,""make"":""Toyota"",""model"":""Camry"",""driverAge"":30}")
    {
        var tenant = DomainTestHelpers.SeedTenant(_db);
        var agent = DomainTestHelpers.SeedUser(_db, tenant.Id, UserRole.Agent);
        var insured = DomainTestHelpers.SeedInsured(_db, tenant.Id, agent.Id);

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            InsuredId = insured.Id,
            SubmittedByUserId = agent.Id,
            LineOfBusiness = LineOfBusiness.Auto,
            Status = SubmissionStatus.InReview,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ExpirationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
            RiskData = riskDataJson
        };
        _db.Submissions.Add(submission);
        _db.SaveChanges();

        return (tenant.Id, submission);
    }

    // ── No rate table ─────────────────────────────────────────────────────────
    [Fact]
    public async Task RateAsync_ThrowsInvalidOperation_WhenNoActiveRateTable()
    {
        var (tenantId, submission) = SeedAutoSubmission();
        // No rate table seeded

        var act = async () => await _sut.RateAsync(submission, tenantId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No active rate table*");
    }

    // ── Base rate only (no factors) ───────────────────────────────────────────
    [Fact]
    public async Task RateAsync_ReturnsBaseRate_WhenNoFactorsApply()
    {
        var (tenantId, submission) = SeedAutoSubmission();
        DomainTestHelpers.SeedRateTable(_db, tenantId,
            baseRate: 1000m, taxRate: 0.10m, flatFee: 50m, factorsJson: "[]");

        var result = await _sut.RateAsync(submission, tenantId);

        result.TotalPremium.Should().Be(1000m);
        result.Taxes.Should().Be(100m);   // 1000 * 0.10
        result.Fees.Should().Be(50m);
        result.TotalDue.Should().Be(1150m);
        result.Adjustments.Should().BeEmpty();
    }

    // ── Single factor fires ───────────────────────────────────────────────────
    [Fact]
    public async Task RateAsync_AppliesMultiplier_WhenNumericConditionMatches()
    {
        // vehicleYear = 2010 → old vehicle loading should fire (lt 2015 → multiplier 1.20)
        var (tenantId, submission) = SeedAutoSubmission(
            @"{""vehicleYear"":2010}");

        var factors = JsonSerializer.Serialize(new[]
        {
            new { Name = "OldVehicleLoading", FieldName = "vehicleYear", Operator = "lt", FieldValue = "2015", Multiplier = 1.20 }
        });
        DomainTestHelpers.SeedRateTable(_db, tenantId,
            baseRate: 1000m, taxRate: 0.05m, flatFee: 25m, factorsJson: factors);

        var result = await _sut.RateAsync(submission, tenantId);

        result.TotalPremium.Should().Be(1200m);       // 1000 * 1.20
        result.Adjustments.Should().ContainSingle(a => a.Name == "OldVehicleLoading");
    }

    [Fact]
    public async Task RateAsync_DoesNotApplyMultiplier_WhenConditionDoesNotMatch()
    {
        // vehicleYear = 2022 → old vehicle loading should NOT fire
        var (tenantId, submission) = SeedAutoSubmission(
            @"{""vehicleYear"":2022}");

        var factors = JsonSerializer.Serialize(new[]
        {
            new { Name = "OldVehicleLoading", FieldName = "vehicleYear", Operator = "lt", FieldValue = "2015", Multiplier = 1.20 }
        });
        DomainTestHelpers.SeedRateTable(_db, tenantId,
            baseRate: 1000m, taxRate: 0.05m, flatFee: 25m, factorsJson: factors);

        var result = await _sut.RateAsync(submission, tenantId);

        result.TotalPremium.Should().Be(1000m);
        result.Adjustments.Should().BeEmpty();
    }

    // ── Multiple factors ──────────────────────────────────────────────────────
    [Fact]
    public async Task RateAsync_AppliesMultipleFactors_InSequence()
    {
        // vehicleYear = 2010 (old), driverAge = 22 (young) → both fire
        var (tenantId, submission) = SeedAutoSubmission(
            @"{""vehicleYear"":2010,""driverAge"":22}");

        var factors = JsonSerializer.Serialize(new object[]
        {
            new { Name = "OldVehicleLoading",  FieldName = "vehicleYear", Operator = "lt", FieldValue = "2015", Multiplier = 1.20 },
            new { Name = "YoungDriverLoading",  FieldName = "driverAge",   Operator = "lt", FieldValue = "25",   Multiplier = 1.30 }
        });
        DomainTestHelpers.SeedRateTable(_db, tenantId,
            baseRate: 1000m, taxRate: 0.05m, flatFee: 0m, factorsJson: factors);

        var result = await _sut.RateAsync(submission, tenantId);

        // 1000 * 1.20 = 1200; 1200 * 1.30 = 1560
        result.TotalPremium.Should().Be(1560m);
        result.Adjustments.Should().HaveCount(2);
    }

    // ── String equality factor ────────────────────────────────────────────────
    [Fact]
    public async Task RateAsync_AppliesMultiplier_WhenStringConditionMatches()
    {
        var (tenantId, submission) = SeedAutoSubmission(
            @"{""make"":""Ferrari""}");

        var factors = JsonSerializer.Serialize(new[]
        {
            new { Name = "ExoticVehicle", FieldName = "make", Operator = "eq", FieldValue = "Ferrari", Multiplier = 2.00 }
        });
        DomainTestHelpers.SeedRateTable(_db, tenantId,
            baseRate: 1000m, taxRate: 0.05m, flatFee: 0m, factorsJson: factors);

        var result = await _sut.RateAsync(submission, tenantId);

        result.TotalPremium.Should().Be(2000m);
        result.Adjustments.Should().ContainSingle(a => a.Name == "ExoticVehicle");
    }

    // ── Missing field → factor skipped ───────────────────────────────────────
    [Fact]
    public async Task RateAsync_SkipsFactor_WhenRiskDataFieldIsMissing()
    {
        var (tenantId, submission) = SeedAutoSubmission(@"{}");  // no vehicleYear

        var factors = JsonSerializer.Serialize(new[]
        {
            new { Name = "OldVehicleLoading", FieldName = "vehicleYear", Operator = "lt", FieldValue = "2015", Multiplier = 1.20 }
        });
        DomainTestHelpers.SeedRateTable(_db, tenantId,
            baseRate: 1000m, taxRate: 0m, flatFee: 0m, factorsJson: factors);

        var result = await _sut.RateAsync(submission, tenantId);

        result.TotalPremium.Should().Be(1000m);
        result.Adjustments.Should().BeEmpty();
    }

    // ── Rate table version — highest active wins ──────────────────────────────
    [Fact]
    public async Task RateAsync_UsesHighestVersionTable_WhenMultipleActive()
    {
        var (tenantId, submission) = SeedAutoSubmission();

        // v1: baseRate 800
        DomainTestHelpers.SeedRateTable(_db, tenantId, baseRate: 800m);

        // v2: baseRate 1200 (manually set TableVersion to avoid unique constraint)
        var v2 = new RateTable
        {
            TenantId = tenantId,
            LineOfBusiness = LineOfBusiness.Auto,
            ProductCode = "AUTO-STANDARD",
            TableVersion = 2,
            IsActive = true,
            BaseRate = 1200m,
            TaxRate = 0.05m,
            FlatFee = 25m,
            FactorsJson = "[]",
            EffectiveFrom = DateTimeOffset.UtcNow
        };
        _db.RateTables.Add(v2);
        _db.SaveChanges();

        var result = await _sut.RateAsync(submission, tenantId);

        result.BaseRate.Should().Be(1200m);
        result.RateTableVersion.Should().Be(2);
    }

    // ── Inactive table ignored ────────────────────────────────────────────────
    [Fact]
    public async Task RateAsync_Throws_WhenOnlyTableIsInactive()
    {
        var (tenantId, submission) = SeedAutoSubmission();
        DomainTestHelpers.SeedRateTable(_db, tenantId, isActive: false);

        var act = async () => await _sut.RateAsync(submission, tenantId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── Rounding ──────────────────────────────────────────────────────────────
    [Fact]
    public async Task RateAsync_RoundsTaxesToTwoDecimalPlaces()
    {
        var (tenantId, submission) = SeedAutoSubmission();
        // baseRate 333.33, taxRate 0.10 → taxes = 33.333 → rounds to 33.33
        DomainTestHelpers.SeedRateTable(_db, tenantId, baseRate: 333.33m, taxRate: 0.10m, flatFee: 0m);

        var result = await _sut.RateAsync(submission, tenantId);

        result.Taxes.Should().Be(33.33m);
    }

    public void Dispose() => _db.Dispose();
}
