using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

/// <summary>
/// A single conditional multiplier stored in a RateTable's FactorsJson.
/// The condition is evaluated against the submission's RiskData JSON.
/// </summary>
public record RatingFactor(
    /// <summary>Human-readable name for the factor, e.g. "OldVehicleLoading".</summary>
    string Name,
    /// <summary>JSON field path in RiskData to evaluate, e.g. "vehicleYear".</summary>
    string FieldName,
    /// <summary>Comparison operator: "lt" | "lte" | "gt" | "gte" | "eq" | "neq".</summary>
    string Operator,
    /// <summary>Threshold value to compare against as a string.</summary>
    string FieldValue,
    /// <summary>Multiplier applied to the running premium when the condition is true.</summary>
    decimal Multiplier);

/// <summary>Records which factors fired during rating for transparency.</summary>
public record RatingAdjustment(string Name, decimal Multiplier);

/// <summary>Result of a rating computation.</summary>
public record RatingResult(
    Guid RateTableId,
    int RateTableVersion,
    decimal BaseRate,
    IReadOnlyList<RatingAdjustment> Adjustments,
    decimal TotalPremium,
    decimal Taxes,
    decimal Fees,
    decimal TotalDue);

public interface IRatingEngine
{
    /// <summary>
    /// Rates a submission using the active rate table for its line of business.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no active rate table exists for the submission's LOB.
    /// </exception>
    Task<RatingResult> RateAsync(Submission submission, Guid tenantId, CancellationToken ct = default);
}
