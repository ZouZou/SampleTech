using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Models;

namespace SampleTech.Api.Data;

/// <summary>
/// Seeds essential bootstrap data (platform admin user).
/// Safe to run repeatedly — idempotent checks guard against duplicates.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config, ILogger logger)
    {
        // Ensure schema is up to date
        await db.Database.MigrateAsync();

        await SeedPlatformAdminAsync(db, config, logger);
    }

    private static async Task SeedPlatformAdminAsync(AppDbContext db, IConfiguration config, ILogger logger)
    {
        var adminEmail = config["Seed:AdminEmail"] ?? "admin@sampletech.com";

        var exists = await db.Users.AnyAsync(u => u.Email == adminEmail && u.Role == UserRole.Admin);
        if (exists) return;

        var adminPassword = config["Seed:AdminPassword"] ?? "Admin@123456!";

        db.Users.Add(new User
        {
            Email = adminEmail,
            FirstName = "Platform",
            LastName = "Admin",
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            TenantId = null, // Platform-level admin, not scoped to a tenant
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword)
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded platform admin user: {Email}", adminEmail);
    }
}
