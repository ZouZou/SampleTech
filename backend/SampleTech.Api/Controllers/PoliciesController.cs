using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleTech.Api.Authorization;
using SampleTech.Api.Models;
using SampleTech.Api.Services;

namespace SampleTech.Api.Controllers;

[ApiController]
[Route("api/policies")]
[Authorize(Policy = Policies.AnyRole)]
public class PoliciesController(IPolicyService policyService, TenantContext tenantContext) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private UserRole Role => Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role)!);
    private Guid TenantId => tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context not resolved.");

    /// <summary>List policies for the tenant. Role-filtered: agents/brokers see their own, clients see their insured's.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PolicySummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var policies = await policyService.ListAsync(TenantId, UserId, Role, ct);
        return Ok(policies);
    }

    /// <summary>Get a specific policy by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PolicyDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var policy = await policyService.GetByIdAsync(id, TenantId, UserId, Role, ct);
        return policy is null ? NotFound() : Ok(policy);
    }

    /// <summary>Create a new policy (starts as Draft). Underwriters and admins only.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<PolicyDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePolicyRequest body, CancellationToken ct)
    {
        var policy = await policyService.CreateAsync(body, TenantId, UserId, Role, ct);
        return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policy);
    }

    /// <summary>Update policy metadata (expiration, premium, agent assignment). Underwriters and admins only.</summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<PolicyDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePolicyRequest body, CancellationToken ct)
    {
        var policy = await policyService.UpdateAsync(id, body, TenantId, UserId, Role, ct);
        return policy is null ? NotFound() : Ok(policy);
    }

    /// <summary>Transition policy through its lifecycle state machine. Underwriters and admins only.</summary>
    [HttpPost("{id:guid}/transition")]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [ProducesResponseType<PolicyDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transition(Guid id, [FromBody] TransitionPolicyStatusRequest body, CancellationToken ct)
    {
        try
        {
            var policy = await policyService.TransitionStatusAsync(id, body, TenantId, UserId, Role, ct);
            return policy is null ? NotFound() : Ok(policy);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>List documents attached to a policy.</summary>
    [HttpGet("{id:guid}/documents")]
    [ProducesResponseType<IReadOnlyList<PolicyDocumentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken ct)
    {
        var docs = await policyService.GetDocumentsAsync(id, TenantId, ct);
        return Ok(docs);
    }

    /// <summary>Attach a PDF or endorsement document to a policy. Underwriters and admins only.</summary>
    [HttpPost("{id:guid}/documents")]
    [Authorize(Policy = Policies.UnderwriterOrAbove)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<PolicyDocumentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
    public async Task<IActionResult> AttachDocument(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        var allowedTypes = new[] { "application/pdf", "image/png", "image/jpeg", "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(new { error = "Only PDF, PNG, JPEG, and Word documents are accepted." });

        await using var stream = file.OpenReadStream();
        var doc = await policyService.AttachDocumentAsync(
            id, file.FileName, file.ContentType, stream, file.Length, TenantId, UserId, ct);

        if (doc is null) return NotFound();
        return CreatedAtAction(nameof(GetDocuments), new { id }, doc);
    }
}
