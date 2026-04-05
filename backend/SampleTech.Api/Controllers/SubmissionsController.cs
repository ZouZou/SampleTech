using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleTech.Api.Authorization;
using SampleTech.Api.Models;
using SampleTech.Api.Services;

namespace SampleTech.Api.Controllers;

[ApiController]
[Route("api/submissions")]
[Authorize(Policy = Policies.StaffOnly)]
public class SubmissionsController(ISubmissionService submissionService, TenantContext tenantContext) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
    private Guid TenantId => tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context not resolved.");

    /// <summary>List submissions. Agents/brokers see their own; underwriters see their queue.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SubmissionSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await submissionService.ListAsync(TenantId, UserId, Role, ct);
        return Ok(result);
    }

    /// <summary>Get submission by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<SubmissionDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await submissionService.GetByIdAsync(id, TenantId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new submission (draft). Agents and brokers only.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AgentOrAbove)]
    [ProducesResponseType<SubmissionDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubmissionRequest body, CancellationToken ct)
    {
        var result = await submissionService.CreateAsync(body, TenantId, UserId, Role, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update submission metadata (underwriter assignment, notes).</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType<SubmissionDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubmissionRequest body, CancellationToken ct)
    {
        var result = await submissionService.UpdateAsync(id, body, TenantId, UserId, Role, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Transition submission status through the underwriting workflow.</summary>
    [HttpPost("{id:guid}/transition")]
    [ProducesResponseType<SubmissionDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transition(Guid id, [FromBody] TransitionSubmissionStatusRequest body, CancellationToken ct)
    {
        try
        {
            var result = await submissionService.TransitionStatusAsync(id, body, TenantId, UserId, Role, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
