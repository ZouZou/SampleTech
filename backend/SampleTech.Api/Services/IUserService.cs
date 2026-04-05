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

    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Creates a password reset token and returns it. In production, this would be emailed.</summary>
    Task<string?> RequestPasswordResetAsync(
        string email,
        string? ipAddress = null,
        CancellationToken ct = default);

    Task<bool> ResetPasswordAsync(
        string token,
        string newPassword,
        string? ipAddress = null,
        CancellationToken ct = default);

    Task<UserDto> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        UserRole role,
        Guid? tenantId,
        string? password,
        CancellationToken ct = default);

    Task<bool> UpdateUserStatusAsync(Guid userId, UserStatus status, CancellationToken ct = default);
    Task<bool> UpdateUserRoleAsync(Guid userId, UserRole role, CancellationToken ct = default);
}
