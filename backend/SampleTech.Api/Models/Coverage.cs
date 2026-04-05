namespace SampleTech.Api.Models;

public enum CoverageType
{
    BodilyInjury,
    PropertyDamage,
    Collision,
    Comprehensive,
    GeneralLiability,
    Umbrella,
    WorkersComp
}

public class Coverage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public CoverageType CoverageType { get; set; }
    public string Description { get; set; } = string.Empty;

    public decimal? LimitPerOccurrence { get; set; }
    public decimal? LimitAggregate { get; set; }
    public decimal? Deductible { get; set; }
    public decimal Premium { get; set; }

    public bool IsActive { get; set; } = true;
    public DateOnly EffectiveDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
