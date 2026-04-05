using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Authorization;
using SampleTech.Api.Data;
using SampleTech.Api.Models;
using SampleTech.Api.Services;

namespace SampleTech.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.AdminOnly)]
public class UsersController(AppDbContext db, IUserService userService) : ControllerBase
{
    public record CreateUserRequest(
        [Required][EmailAddress] string Email,
        [Required][MaxLength(100)] string FirstName,
        [Required][MaxLength(100)] string LastName,
        [Required] UserRole Role,
        Guid? TenantId,
        [MinLength(8)] string? Password);

    public record UpdateStatusRequest([Required] UserStatus Status);
    public record UpdateRoleRequest([Required] UserRole Role);

    public record UserListItem(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        UserRole Role,
        UserStatus Status,
        Guid? TenantId,
        DateTimeOffset? LastLoginAt,
        DateTimeOffset CreatedAt);

    /// <summary>List all users. Supports filtering by role and status.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<UserListItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] UserRole? role,
        [FromQuery] UserStatus? status,
        [FromQuery] Guid? tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = db.Users.AsNoTracking();

        if (role.HasValue) query = query.Where(u => u.Role == role.Value);
        if (status.HasValue) query = query.Where(u => u.Status == status.Value);
        if (tenantId.HasValue) query = query.Where(u => u.TenantId == tenantId.Value);

        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await query.CountAsync(ct);
        var users = await query
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItem(u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.Status, u.TenantId, u.LastLoginAt, u.CreatedAt))
            .ToListAsync(ct);

        return Ok(new { total, page, pageSize, data = users });
    }

    /// <summary>Get a single user by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserListItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserListItem(u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.Status, u.TenantId, u.LastLoginAt, u.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Create a new user (admin use — e.g. inviting a staff member).</summary>
    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest body, CancellationToken ct)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == body.Email.ToLowerInvariant(), ct);
        if (exists)
            return Conflict(new { error = "A user with that email already exists." });

        var user = await userService.CreateUserAsync(
            body.Email, body.FirstName, body.LastName,
            body.Role, body.TenantId, body.Password, ct);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Update a user's status (activate, suspend, deactivate).</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest body, CancellationToken ct)
    {
        var updated = await userService.UpdateUserStatusAsync(id, body.Status, ct);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Change a user's role.</summary>
    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest body, CancellationToken ct)
    {
        var updated = await userService.UpdateUserRoleAsync(id, body.Role, ct);
        return updated ? NoContent() : NotFound();
    }
}
