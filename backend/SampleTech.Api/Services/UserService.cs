using System.Security.Cryptography;
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
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<LoginResult?> LoginAsync(
        string email,
        string password,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant() && u.Status == UserStatus.Active, ct);

        // Account locked check
        if (user is not null && user.LockedUntil.HasValue && user.LockedUntil > DateTimeOffset.UtcNow)
        {
            await auditService.LogAsync(new AuditEventContext(
                AuthEventType.LoginFailed,
                Email: email,
                UserId: user.Id,
                TenantId: user.TenantId,
                IpAddress: ipAddress,
                UserAgent: userAgent,
                Metadata: "{\"reason\":\"account_locked\"}"), ct);
            return null;
        }

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            if (user is not null)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);
                    await db.SaveChangesAsync(ct);
                    await auditService.LogAsync(new AuditEventContext(
                        AuthEventType.AccountLocked,
                        Email: user.Email,
                        UserId: user.Id,
                        TenantId: user.TenantId,
                        IpAddress: ipAddress,
                        UserAgent: userAgent), ct);
                }
                else
                {
                    await db.SaveChangesAsync(ct);
                }
            }

            await auditService.LogAsync(new AuditEventContext(
                AuthEventType.LoginFailed,
                Email: email,
                UserId: user?.Id,
                TenantId: user?.TenantId,
                IpAddress: ipAddress,
                UserAgent: userAgent), ct);

            return null;
        }

        // Successful login: reset lockout state and update last login
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

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

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user is null ? null : ToDto(user);
    }

    public async Task<string?> RequestPasswordResetAsync(
        string email,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant() && u.Status == UserStatus.Active, ct);

        // Return null silently if user not found — don't leak user existence
        if (user is null) return null;

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var expiryMinutes = config.GetSection("PasswordReset").GetValue<int>("ExpiryMinutes", 60);

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            Token = rawToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
        });
        await db.SaveChangesAsync(ct);

        return rawToken;
    }

    public async Task<bool> ResetPasswordAsync(
        string token,
        string newPassword,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var prt = await db.PasswordResetTokens
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Token == token && !p.IsUsed, ct);

        if (prt is null || prt.ExpiresAt <= DateTimeOffset.UtcNow)
            return false;

        prt.IsUsed = true;
        prt.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        prt.User.FailedLoginAttempts = 0;
        prt.User.LockedUntil = null;
        prt.User.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UserDto> CreateUserAsync(
        string email,
        string firstName,
        string lastName,
        UserRole role,
        Guid? tenantId,
        string? password,
        CancellationToken ct = default)
    {
        var user = new User
        {
            Email = email.ToLowerInvariant(),
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            TenantId = tenantId,
            Status = UserStatus.Active,
            PasswordHash = password is not null ? BCrypt.Net.BCrypt.HashPassword(password) : null
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return ToDto(user);
    }

    public async Task<bool> UpdateUserStatusAsync(Guid userId, UserStatus status, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([userId], ct);
        if (user is null) return false;

        user.Status = status;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, UserRole role, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([userId], ct);
        if (user is null) return false;

        user.Role = role;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
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
            ToDto(user));
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Role, user.TenantId);
}
