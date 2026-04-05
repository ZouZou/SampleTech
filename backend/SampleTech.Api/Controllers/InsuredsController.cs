using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleTech.Api.Authorization;
using SampleTech.Api.Models;
using SampleTech.Api.Services;

namespace SampleTech.Api.Controllers;

[ApiController]
[Route("api/insureds")]
[Authorize(Policy = Policies.StaffOnly)]
public class InsuredsController(IInsuredService insuredService, TenantContext tenantContext) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
    private Guid TenantId => tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context not resolved.");

    /// <summary>List all insureds for the tenant. Agents/brokers see only their own.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InsuredSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await insuredService.ListAsync(TenantId, UserId, Role, ct);
        return Ok(result);
    }

    /// <summary>Get insured by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<InsuredDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await insuredService.GetByIdAsync(id, TenantId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new insured. Agents and brokers only.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.AgentOrAbove)]
    [ProducesResponseType<InsuredDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInsuredRequest body, CancellationToken ct)
    {
        var result = await insuredService.CreateAsync(body, TenantId, UserId, Role, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update insured contact details or assignment.</summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Policies.AgentOrAbove)]
    [ProducesResponseType<InsuredDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInsuredRequest body, CancellationToken ct)
    {
        var result = await insuredService.UpdateAsync(id, body, TenantId, UserId, Role, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
