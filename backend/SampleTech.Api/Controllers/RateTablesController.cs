using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleTech.Api.Authorization;
using SampleTech.Api.Models;
using SampleTech.Api.Services;

namespace SampleTech.Api.Controllers;

[ApiController]
[Route("api/rate-tables")]
[Authorize(Policy = Policies.AdminOnly)]
public class RateTablesController(IRateTableService rateTableService, TenantContext tenantContext) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
    private Guid TenantId => tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context not resolved.");

    /// <summary>List rate tables, optionally filtered by line of business.</summary>
    [HttpGet]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<IReadOnlyList<RateTableDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] LineOfBusiness? lob, CancellationToken ct)
    {
        var result = await rateTableService.ListAsync(TenantId, lob, ct);
        return Ok(result);
    }

    /// <summary>Get a specific rate table by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<RateTableDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await rateTableService.GetByIdAsync(id, TenantId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new rate table. Admin only.</summary>
    [HttpPost]
    [ProducesResponseType<RateTableDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRateTableRequest body, CancellationToken ct)
    {
        var result = await rateTableService.CreateAsync(body, TenantId, UserId, Role, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing rate table. Admin only.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<RateTableDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRateTableRequest body, CancellationToken ct)
    {
        var result = await rateTableService.UpdateAsync(id, body, TenantId, UserId, Role, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
