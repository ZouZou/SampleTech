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

    public record ForgotPasswordRequest([Required][EmailAddress] string Email);

    public record ResetPasswordRequest(
        [Required] string Token,
        [Required][MinLength(8)] string NewPassword);

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
            return Unauthorized(new { error = "Invalid email or password, or account is locked." });

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

    /// <summary>Return the currently authenticated user's profile.</summary>
    [HttpGet("me")]
    [Authorize(Policy = Policies.AnyRole)]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var user = await userService.GetByIdAsync(userId, ct);
        if (user is null) return Unauthorized();

        return Ok(user);
    }

    /// <summary>
    /// Request a password reset token.
    /// In production this would send an email; in development the token is returned directly.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest body, CancellationToken ct)
    {
        var token = await userService.RequestPasswordResetAsync(body.Email, ClientIp, ct);

        // Always return 200 to avoid email enumeration. Return token only in non-production.
        if (HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsProduction())
            return Ok(new { message = "If an account with that email exists, a reset link has been sent." });

        return Ok(new { message = "Reset token issued.", resetToken = token });
    }

    /// <summary>Reset password using a previously issued reset token.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest body, CancellationToken ct)
    {
        var success = await userService.ResetPasswordAsync(body.Token, body.NewPassword, ClientIp, ct);
        if (!success)
            return BadRequest(new { error = "Invalid or expired reset token." });

        return NoContent();
    }
}
