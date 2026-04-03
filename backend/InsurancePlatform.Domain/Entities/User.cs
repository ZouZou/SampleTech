using InsurancePlatform.Domain.Enums;

namespace InsurancePlatform.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
