using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleTech.Api.Authorization;
using SampleTech.Api.Models;
using SampleTech.Api.Services;

namespace SampleTech.Api.Controllers;

[ApiController]
[Route("api/claims")]
[Authorize(Policy = Policies.AnyRole)]
public class ClaimsController(IClaimService claimService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);

    /// <summary>List claims. Clients see their own; agents/brokers see policy-linked; underwriters see all.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClaimSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var claims = await claimService.ListAsync(UserId, Role, ct);
        return Ok(claims);
    }

    /// <summary>Get a specific claim by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ClaimDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var claim = await claimService.GetByIdAsync(id, UserId, Role, ct);
        return claim is null ? NotFound() : Ok(claim);
    }

    /// <summary>File a new claim. Clients, agents, and brokers.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AgentOrAbove)]
    [ProducesResponseType<ClaimDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> File([FromBody] FileClaim body, CancellationToken ct)
    {
        var claim = await claimService.FileAsync(body, UserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = claim.Id }, claim);
    }

    /// <summary>Update claim status and review notes. Underwriters and admins only.</summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<ClaimDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClaimRequest body, CancellationToken ct)
    {
        var claim = await claimService.UpdateAsync(id, body, UserId, ct);
        return claim is null ? NotFound() : Ok(claim);
    }
}
