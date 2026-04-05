namespace SampleTech.Api.Models;

/// <summary>
/// Configurable rate table that drives premium calculation for a given line of business.
/// Each tenant maintains its own tables; the active table with the highest version is used at rating time.
/// </summary>
public class RateTable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public LineOfBusiness LineOfBusiness { get; set; }

    /// <summary>Logical product identifier within a LOB, e.g. "AUTO-STANDARD".</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>Monotonically incrementing within (tenant, LOB, ProductCode). Starts at 1.</summary>
    public int TableVersion { get; set; } = 1;

    /// <summary>Only one active table per (tenant, LOB, ProductCode) is used for rating.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Starting base premium before any risk-factor adjustments.</summary>
    public decimal BaseRate { get; set; }

    /// <summary>Tax rate as a decimal fraction, e.g. 0.05 for 5 %.</summary>
    public decimal TaxRate { get; set; }

    /// <summary>Flat per-policy fee (dollars).</summary>
    public decimal FlatFee { get; set; }

    /// <summary>
    /// JSON array of <see cref="SampleTech.Api.Services.RatingFactor"/> objects.
    /// Each factor describes a conditional multiplier applied to the running premium.
    /// </summary>
    public string FactorsJson { get; set; } = "[]";

    public DateTimeOffset EffectiveFrom { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EffectiveTo { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
