using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Models;

namespace SampleTech.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // ── Auth / identity ────────────────────────────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ── Audit ──────────────────────────────────────────────────────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<MutationAuditLog> MutationAuditLogs => Set<MutationAuditLog>();

    // ── Domain ─────────────────────────────────────────────────────────────────
    public DbSet<Insured> Insureds => Set<Insured>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Coverage> Coverages => Set<Coverage>();
    public DbSet<PolicyDocument> PolicyDocuments => Set<PolicyDocument>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<RateTable> RateTables => Set<RateTable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Tenant ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Name).HasMaxLength(255).IsRequired();
            e.Property(t => t.Slug).HasMaxLength(100).IsRequired();
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.Plan).HasConversion<string>();
            e.Property(t => t.LogoUrl).HasMaxLength(2048);
            e.Property(t => t.PrimaryDomain).HasMaxLength(253);
        });

        // ── User ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            // Email unique per tenant (tenantId, email) — global uniqueness for admin users
            e.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
            e.Property(u => u.Email).HasMaxLength(320).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Role).HasConversion<string>();
            e.Property(u => u.Status).HasConversion<string>();

            e.HasOne(u => u.Tenant)
             .WithMany(t => t.Users)
             .HasForeignKey(u => u.TenantId)
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);
        });

        // ── RefreshToken ────────────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
            e.HasOne(r => r.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLog (auth events) — append-only ────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.OccurredAt);
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.TenantId);
            e.Property(a => a.EventType).HasConversion<string>();
            e.Property(a => a.Email).HasMaxLength(320);
            e.Property(a => a.IpAddress).HasMaxLength(45);
            e.Property(a => a.UserAgent).HasMaxLength(512);
        });

        // ── MutationAuditLog (domain mutations) — append-only ──────────────────
        modelBuilder.Entity<MutationAuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => new { a.TenantId, a.EntityType, a.EntityId });
            e.HasIndex(a => a.ActorUserId);
            e.HasIndex(a => a.CreatedAt);
            e.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            e.Property(a => a.Action).HasConversion<string>();
            e.Property(a => a.ActorRole).HasMaxLength(50);
            e.Property(a => a.IpAddress).HasMaxLength(45);
            e.Property(a => a.UserAgent).HasMaxLength(512);
        });

        // ── Insured ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Insured>(e =>
        {
            e.HasKey(i => i.Id);
            e.HasIndex(i => new { i.TenantId, i.AssignedAgentId });
            e.Property(i => i.Type).HasConversion<string>();
            e.Property(i => i.Email).HasMaxLength(320).IsRequired();
            e.Property(i => i.Phone).HasMaxLength(30);
            e.Property(i => i.FirstName).HasMaxLength(100);
            e.Property(i => i.LastName).HasMaxLength(100);
            e.Property(i => i.BusinessName).HasMaxLength(255);
            e.Property(i => i.TaxId).HasMaxLength(100);

            e.HasOne(i => i.Tenant)
             .WithMany(t => t.Insureds)
             .HasForeignKey(i => i.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.LinkedUser)
             .WithMany()
             .HasForeignKey(i => i.LinkedUserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);

            e.HasOne(i => i.AssignedAgent)
             .WithMany()
             .HasForeignKey(i => i.AssignedAgentId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── Submission ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Submission>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.TenantId, s.Status });
            e.HasIndex(s => s.InsuredId);
            e.Property(s => s.Status).HasConversion<string>();
            e.Property(s => s.LineOfBusiness).HasConversion<string>();
            e.Property(s => s.Notes).HasMaxLength(4000);
            e.Property(s => s.DeclineReason).HasMaxLength(2000);

            e.HasOne(s => s.Tenant)
             .WithMany()
             .HasForeignKey(s => s.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Insured)
             .WithMany(i => i.Submissions)
             .HasForeignKey(s => s.InsuredId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.SubmittedByUser)
             .WithMany()
             .HasForeignKey(s => s.SubmittedByUserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.AssignedUnderwriter)
             .WithMany()
             .HasForeignKey(s => s.AssignedUnderwriterId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── Quote ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<Quote>(e =>
        {
            e.HasKey(q => q.Id);
            e.HasIndex(q => q.SubmissionId);
            e.HasIndex(q => new { q.Status, q.QuoteExpiryDate });
            e.Property(q => q.Status).HasConversion<string>();
            e.Property(q => q.TotalPremium).HasColumnType("numeric(12,2)");
            e.Property(q => q.Taxes).HasColumnType("numeric(12,2)");
            e.Property(q => q.Fees).HasColumnType("numeric(12,2)");
            e.Property(q => q.TotalDue).HasColumnType("numeric(12,2)");
            e.Property(q => q.Terms).HasMaxLength(4000);

            e.HasOne(q => q.Tenant)
             .WithMany()
             .HasForeignKey(q => q.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(q => q.Submission)
             .WithMany(s => s.Quotes)
             .HasForeignKey(q => q.SubmissionId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(q => q.IssuedByUser)
             .WithMany()
             .HasForeignKey(q => q.IssuedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Policy ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Policy>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.PolicyNumber).IsUnique();
            e.HasIndex(p => new { p.TenantId, p.Status });
            e.Property(p => p.PolicyNumber).HasMaxLength(50).IsRequired();
            e.Property(p => p.Status).HasConversion<string>();
            e.Property(p => p.LineOfBusiness).HasConversion<string>();
            e.Property(p => p.TotalPremium).HasColumnType("numeric(12,2)");
            e.Property(p => p.CancellationReason).HasMaxLength(2000);

            e.HasOne(p => p.Tenant)
             .WithMany()
             .HasForeignKey(p => p.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Quote)
             .WithOne(q => q.Policy)
             .HasForeignKey<Policy>(p => p.QuoteId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);

            e.HasOne(p => p.Insured)
             .WithMany(i => i.Policies)
             .HasForeignKey(p => p.InsuredId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.IssuedByUser)
             .WithMany(u => u.Policies)
             .HasForeignKey(p => p.IssuedByUserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.AssignedAgent)
             .WithMany()
             .HasForeignKey(p => p.AssignedAgentId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);

            e.HasOne(p => p.RenewalPolicy)
             .WithMany()
             .HasForeignKey(p => p.RenewalPolicyId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── Coverage ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Coverage>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CoverageType).HasConversion<string>();
            e.Property(c => c.Description).HasMaxLength(500).IsRequired();
            e.Property(c => c.LimitPerOccurrence).HasColumnType("numeric(12,2)");
            e.Property(c => c.LimitAggregate).HasColumnType("numeric(12,2)");
            e.Property(c => c.Deductible).HasColumnType("numeric(12,2)");
            e.Property(c => c.Premium).HasColumnType("numeric(12,2)");

            e.HasOne(c => c.Policy)
             .WithMany(p => p.Coverages)
             .HasForeignKey(c => c.PolicyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Tenant)
             .WithMany()
             .HasForeignKey(c => c.TenantId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PolicyDocument ──────────────────────────────────────────────────────
        modelBuilder.Entity<PolicyDocument>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.FileName).HasMaxLength(255).IsRequired();
            e.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
            e.Property(d => d.BlobUrl).HasMaxLength(2048).IsRequired();

            e.HasOne(d => d.Policy)
             .WithMany(p => p.Documents)
             .HasForeignKey(d => d.PolicyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Tenant)
             .WithMany()
             .HasForeignKey(d => d.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(d => d.UploadedByUser)
             .WithMany()
             .HasForeignKey(d => d.UploadedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── RateTable ────────────────────────────────────────────────────────────
        modelBuilder.Entity<RateTable>(e =>
        {
            e.HasKey(rt => rt.Id);
            e.HasIndex(rt => new { rt.TenantId, rt.LineOfBusiness, rt.IsActive });
            e.HasIndex(rt => new { rt.TenantId, rt.LineOfBusiness, rt.ProductCode, rt.TableVersion }).IsUnique();
            e.Property(rt => rt.LineOfBusiness).HasConversion<string>();
            e.Property(rt => rt.ProductCode).HasMaxLength(100).IsRequired();
            e.Property(rt => rt.BaseRate).HasColumnType("numeric(12,2)");
            e.Property(rt => rt.TaxRate).HasColumnType("numeric(8,6)");
            e.Property(rt => rt.FlatFee).HasColumnType("numeric(12,2)");

            e.HasOne(rt => rt.Tenant)
             .WithMany()
             .HasForeignKey(rt => rt.TenantId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Claim ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<Claim>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.ClaimNumber).IsUnique();
            e.Property(c => c.ClaimNumber).HasMaxLength(50).IsRequired();
            e.Property(c => c.Status).HasConversion<string>();
            e.Property(c => c.ClaimedAmount).HasColumnType("numeric(18,2)");
            e.Property(c => c.ApprovedAmount).HasColumnType("numeric(18,2)");
            e.Property(c => c.Description).HasMaxLength(2000).IsRequired();

            e.HasOne(c => c.Policy)
             .WithMany(p => p.Claims)
             .HasForeignKey(c => c.PolicyId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Claimant)
             .WithMany(u => u.Claims)
             .HasForeignKey(c => c.ClaimantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.ReviewedBy)
             .WithMany()
             .HasForeignKey(c => c.ReviewedByUserId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
