using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Tests.Helpers;

/// <summary>Shared factory helpers for creating well-formed domain entities in tests.</summary>
public static class DomainTestHelpers
{
    public static Tenant SeedTenant(AppDbContext db, string slug = "test-co")
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Co",
            Slug = slug,
            Status = TenantStatus.Active,
            Plan = TenantPlan.Professional
        };
        db.Tenants.Add(tenant);
        db.SaveChanges();
        return tenant;
    }

    public static User SeedUser(AppDbContext db, Guid tenantId, UserRole role = UserRole.Underwriter)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = $"{role.ToString().ToLower()}@test.com",
            FirstName = "Test",
            LastName = role.ToString(),
            Role = role,
            Status = UserStatus.Active,
            PasswordHash = "hash"
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public static Insured SeedInsured(AppDbContext db, Guid tenantId, Guid? agentId = null)
    {
        var insured = new Insured
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = InsuredType.Individual,
            FirstName = "Alice",
            LastName = "Test",
            Email = "alice@example.com",
            Address = @"{""street"":""1 Main St"",""city"":""Testville"",""state"":""TX"",""zip"":""75001"",""country"":""US""}",
            AssignedAgentId = agentId
        };
        db.Insureds.Add(insured);
        db.SaveChanges();
        return insured;
    }

    public static Submission SeedSubmission(AppDbContext db, Guid tenantId, Guid insuredId, Guid agentId,
        SubmissionStatus status = SubmissionStatus.Submitted)
    {
        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InsuredId = insuredId,
            SubmittedByUserId = agentId,
            LineOfBusiness = LineOfBusiness.Auto,
            Status = status,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ExpirationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
            RiskData = @"{""vehicleYear"":2022,""make"":""Toyota"",""model"":""Camry""}"
        };
        db.Submissions.Add(submission);
        db.SaveChanges();
        return submission;
    }

    public static RateTable SeedRateTable(
        AppDbContext db, Guid tenantId,
        LineOfBusiness lob = LineOfBusiness.Auto,
        string productCode = "AUTO-STANDARD",
        decimal baseRate = 800m,
        decimal taxRate = 0.05m,
        decimal flatFee = 25m,
        string factorsJson = "[]",
        bool isActive = true)
    {
        var rt = new RateTable
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LineOfBusiness = lob,
            ProductCode = productCode,
            TableVersion = 1,
            IsActive = isActive,
            BaseRate = baseRate,
            TaxRate = taxRate,
            FlatFee = flatFee,
            FactorsJson = factorsJson,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1)
        };
        db.RateTables.Add(rt);
        db.SaveChanges();
        return rt;
    }

    public static Policy SeedPolicy(AppDbContext db, Guid tenantId, Guid insuredId, Guid underwriterId,
        PolicyStatus status = PolicyStatus.Active)
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InsuredId = insuredId,
            IssuedByUserId = underwriterId,
            PolicyNumber = $"TEST-AUTO-{DateTimeOffset.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..5].ToUpper()}",
            LineOfBusiness = LineOfBusiness.Auto,
            Status = status,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
            ExpirationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(365)),
            TotalPremium = 1200m
        };
        db.Policies.Add(policy);
        db.SaveChanges();
        return policy;
    }
}
