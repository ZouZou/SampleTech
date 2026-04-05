using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;
using SampleTech.Api.Models;
using System.Text.Json;

namespace SampleTech.Api.Services;

public class RatingEngine(AppDbContext db) : IRatingEngine
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<RatingResult> RateAsync(
        Submission submission, Guid tenantId, CancellationToken ct = default)
    {
        var table = await db.RateTables.AsNoTracking()
            .Where(rt => rt.TenantId == tenantId
                      && rt.LineOfBusiness == submission.LineOfBusiness
                      && rt.IsActive)
            .OrderByDescending(rt => rt.TableVersion)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException(
                $"No active rate table found for line of business '{submission.LineOfBusiness}' " +
                $"in tenant {tenantId}. Configure one via the Rate Tables API.");

        var factors = JsonSerializer.Deserialize<List<RatingFactor>>(table.FactorsJson, JsonOpts) ?? [];

        using var riskDoc = JsonDocument.Parse(submission.RiskData);
        var riskRoot = riskDoc.RootElement;

        var adjustments = new List<RatingAdjustment>();
        decimal runningPremium = table.BaseRate;

        foreach (var factor in factors)
        {
            if (EvaluateCondition(riskRoot, factor))
            {
                runningPremium *= factor.Multiplier;
                adjustments.Add(new RatingAdjustment(factor.Name, factor.Multiplier));
            }
        }

        runningPremium = Math.Round(runningPremium, 2, MidpointRounding.AwayFromZero);
        var taxes = Math.Round(runningPremium * table.TaxRate, 2, MidpointRounding.AwayFromZero);
        var fees = table.FlatFee;
        var totalDue = Math.Round(runningPremium + taxes + fees, 2, MidpointRounding.AwayFromZero);

        return new RatingResult(
            RateTableId: table.Id,
            RateTableVersion: table.TableVersion,
            BaseRate: table.BaseRate,
            Adjustments: adjustments,
            TotalPremium: runningPremium,
            Taxes: taxes,
            Fees: fees,
            TotalDue: totalDue);
    }

    /// <summary>
    /// Evaluates a single factor condition against the submission's risk data.
    /// Numeric comparisons are used when both sides parse as decimals; otherwise string equality is applied.
    /// </summary>
    private static bool EvaluateCondition(JsonElement riskRoot, RatingFactor factor)
    {
        if (!riskRoot.TryGetProperty(factor.FieldName, out var prop))
            return false;

        // Numeric comparison
        if (prop.ValueKind is JsonValueKind.Number && decimal.TryParse(factor.FieldValue, out var threshold))
        {
            var actual = prop.GetDecimal();
            return factor.Operator switch
            {
                "lt"  => actual < threshold,
                "lte" => actual <= threshold,
                "gt"  => actual > threshold,
                "gte" => actual >= threshold,
                "eq"  => actual == threshold,
                "neq" => actual != threshold,
                _     => false
            };
        }

        // String comparison (eq / neq only)
        var actualStr = prop.ValueKind is JsonValueKind.String
            ? prop.GetString() ?? string.Empty
            : prop.ToString();

        return factor.Operator switch
        {
            "eq"  => string.Equals(actualStr, factor.FieldValue, StringComparison.OrdinalIgnoreCase),
            "neq" => !string.Equals(actualStr, factor.FieldValue, StringComparison.OrdinalIgnoreCase),
            _     => false
        };
    }
}
