using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleTech.Api.Authorization;
using SampleTech.Api.Models;
using SampleTech.Api.Services;

namespace SampleTech.Api.Controllers;

[ApiController]
[Route("api/quotes")]
[Authorize(Policy = Policies.StaffOnly)]
public class QuotesController(IQuoteService quoteService, TenantContext tenantContext) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
    private Guid TenantId => tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context not resolved.");

    /// <summary>List all quotes for a submission.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<QuoteSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListBySubmission([FromQuery] Guid submissionId, CancellationToken ct)
    {
        var result = await quoteService.ListBySubmissionAsync(submissionId, TenantId, ct);
        return Ok(result);
    }

    /// <summary>Get a specific quote by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<QuoteDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await quoteService.GetByIdAsync(id, TenantId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Issue a new quote for a submission. Underwriters and admins only.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<QuoteDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateQuoteRequest body, CancellationToken ct)
    {
        try
        {
            var result = await quoteService.CreateAsync(body, TenantId, UserId, Role, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Transition quote status (issue, accept, decline, expire). Underwriters and admins only.</summary>
    [HttpPost("{id:guid}/transition")]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<QuoteDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transition(Guid id, [FromBody] TransitionQuoteStatusRequest body, CancellationToken ct)
    {
        try
        {
            var result = await quoteService.TransitionStatusAsync(id, body, TenantId, UserId, Role, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Preview the rating result for a submission without creating a quote.
    /// Returns the computed premium breakdown based on the active rate table for the submission's LOB.
    /// </summary>
    [HttpPost("rate-preview")]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<RatingResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RatePreview([FromBody] RatePreviewRequest body, CancellationToken ct)
    {
        try
        {
            var result = await quoteService.RatePreviewAsync(body, TenantId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Re-rate an existing Draft or Issued quote using the current active rate table.
    /// Increments the quote version. Underwriters and admins only.
    /// </summary>
    [HttpPost("{id:guid}/rerate")]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<QuoteDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rerate(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await quoteService.RerateAsync(id, TenantId, UserId, Role, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
