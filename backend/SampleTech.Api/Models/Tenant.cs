namespace SampleTech.Api.Models;

public enum TenantStatus
{
    Onboarding,
    Active,
    Suspended
}

public enum TenantPlan
{
    Starter,
    Professional,
    Enterprise
}

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantStatus Status { get; set; } = TenantStatus.Onboarding;
    public TenantPlan Plan { get; set; } = TenantPlan.Starter;
    public string? LogoUrl { get; set; }
    public string? PrimaryDomain { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Insured> Insureds { get; set; } = [];
}
