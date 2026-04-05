using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleTech.Api.Authorization;
using SampleTech.Api.Services;

namespace SampleTech.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService) : ControllerBase
{
    public record LoginRequest(
        [Required][EmailAddress] string Email,
        [Required][MinLength(8)] string Password);

    public record RefreshRequest([Required] string RefreshToken);

    private string? ClientIp =>
        HttpContext.Connection.RemoteIpAddress?.ToString()
        ?? Request.Headers["X-Forwarded-For"].FirstOrDefault();

    private string? ClientUserAgent =>
        Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;

    /// <summary>Authenticate and receive JWT + refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var result = await userService.LoginAsync(body.Email, body.Password, ClientIp, ClientUserAgent, ct);
        if (result is null)
            return Unauthorized(new { error = "Invalid email or password." });

        return Ok(result);
    }

    /// <summary>Exchange a valid refresh token for a new token pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest body, CancellationToken ct)
    {
        var result = await userService.RefreshAsync(body.RefreshToken, ClientIp, ClientUserAgent, ct);
        if (result is null)
            return Unauthorized(new { error = "Invalid or expired refresh token." });

        return Ok(result);
    }

    /// <summary>Revoke the current refresh token (logout).</summary>
    [HttpPost("logout")]
    [Authorize(Policy = Policies.AnyRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest body, CancellationToken ct)
    {
        var actorId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;
        await userService.RevokeRefreshTokenAsync(body.RefreshToken, actorId, ClientIp, ClientUserAgent, ct);
        return NoContent();
    }
}
