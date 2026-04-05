using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public record LoginResult(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiry, UserDto User);

public record UserDto(Guid Id, string Email, string FirstName, string LastName, UserRole Role, Guid? TenantId);

public interface IUserService
{
    Task<LoginResult?> LoginAsync(
        string email,
        string password,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default);

    Task<LoginResult?> RefreshAsync(
        string refreshToken,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default);

    Task RevokeRefreshTokenAsync(
        string refreshToken,
        Guid? actorUserId = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default);
}
