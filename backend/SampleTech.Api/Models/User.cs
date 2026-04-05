namespace SampleTech.Api.Models;

public enum UserRole
{
    Admin,
    Underwriter,
    Agent,
    Broker,
    Client
}

public enum UserStatus
{
    Invited,
    Active,
    Suspended,
    Deactivated
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Null for platform-level Admin users who are not scoped to a tenant.</summary>
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>Null for SSO-only users.</summary>
    public string? PasswordHash { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Invited;
    public bool MfaEnabled { get; set; } = false;
    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Policy> Policies { get; set; } = [];
    public ICollection<Claim> Claims { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User User { get; set; } = null!;
}
