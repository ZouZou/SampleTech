using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;
using SampleTech.Api.Models;

namespace SampleTech.Api.Services;

public class UserService(
    AppDbContext db,
    ITokenService tokenService,
    IConfiguration config,
    IAuditService auditService) : IUserService
{
    public async Task<LoginResult?> LoginAsync(
        string email,
        string password,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant() && u.Status == UserStatus.Active, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            await auditService.LogAsync(new AuditEventContext(
                AuthEventType.LoginFailed,
                Email: email,
                UserId: user?.Id,
                TenantId: user?.TenantId,
                IpAddress: ipAddress,
                UserAgent: userAgent), ct);

            return null;
        }

        var result = await IssueTokensAsync(user, ct);

        await auditService.LogAsync(new AuditEventContext(
            AuthEventType.LoginSuccess,
            Email: user.Email,
            UserId: user.Id,
            TenantId: user.TenantId,
            IpAddress: ipAddress,
            UserAgent: userAgent), ct);

        return result;
    }

    public async Task<LoginResult?> RefreshAsync(
        string refreshToken,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        var stored = await db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked, ct);

        if (stored is null || stored.ExpiresAt <= DateTimeOffset.UtcNow || stored.User.Status != UserStatus.Active)
        {
            await auditService.LogAsync(new AuditEventContext(
                AuthEventType.TokenRefreshFailed,
                UserId: stored?.User.Id,
                TenantId: stored?.User.TenantId,
                IpAddress: ipAddress,
                UserAgent: userAgent), ct);

            return null;
        }

        stored.IsRevoked = true;
        await db.SaveChangesAsync(ct);

        var result = await IssueTokensAsync(stored.User, ct);

        await auditService.LogAsync(new AuditEventContext(
            AuthEventType.TokenRefreshed,
            Email: stored.User.Email,
            UserId: stored.User.Id,
            TenantId: stored.User.TenantId,
            IpAddress: ipAddress,
            UserAgent: userAgent), ct);

        return result;
    }

    public async Task RevokeRefreshTokenAsync(
        string refreshToken,
        Guid? actorUserId = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        var stored = await db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);

        if (stored is null) return;

        stored.IsRevoked = true;
        await db.SaveChangesAsync(ct);

        await auditService.LogAsync(new AuditEventContext(
            AuthEventType.Logout,
            Email: stored.User.Email,
            UserId: actorUserId ?? stored.UserId,
            TenantId: stored.User.TenantId,
            IpAddress: ipAddress,
            UserAgent: userAgent), ct);
    }

    private async Task<LoginResult> IssueTokensAsync(User user, CancellationToken ct)
    {
        var pair = tokenService.GenerateTokenPair(user);
        var expiryDays = config.GetSection("Jwt").GetValue<int>("RefreshTokenExpiryDays", 30);

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = pair.RefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(expiryDays)
        });
        await db.SaveChangesAsync(ct);

        return new LoginResult(
            pair.AccessToken,
            pair.RefreshToken,
            pair.AccessTokenExpiry,
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role, user.TenantId));
    }
}
