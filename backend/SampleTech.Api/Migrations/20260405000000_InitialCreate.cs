using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleTech.Api.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Onboarding"),
                Plan = table.Column<string>(type: "text", nullable: false, defaultValue: "Starter"),
                LogoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                PrimaryDomain = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Tenants", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Tenants_Slug",
            table: "Tenants",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Role = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                MfaEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                LockedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.ForeignKey(
                    name: "FK_Users_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_TenantId_Email",
            table: "Users",
            columns: new[] { "TenantId", "Email" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Token = table.Column<string>(type: "text", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_RefreshTokens_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RefreshTokens_Token",
            table: "RefreshTokens",
            column: "Token",
            unique: true);

        migrationBuilder.CreateTable(
            name: "PasswordResetTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_PasswordResetTokens_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PasswordResetTokens_Token",
            table: "PasswordResetTokens",
            column: "Token",
            unique: true);

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EventType = table.Column<string>(type: "text", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Metadata = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AuditLogs", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_AuditLogs_OccurredAt", table: "AuditLogs", column: "OccurredAt");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_UserId", table: "AuditLogs", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_AuditLogs_TenantId", table: "AuditLogs", column: "TenantId");

        migrationBuilder.CreateTable(
            name: "MutationAuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                ActorRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                Action = table.Column<string>(type: "text", nullable: false),
                PreviousState = table.Column<string>(type: "text", nullable: true),
                NextState = table.Column<string>(type: "text", nullable: true),
                IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_MutationAuditLogs", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_MutationAuditLogs_TenantId_EntityType_EntityId",
            table: "MutationAuditLogs",
            columns: new[] { "TenantId", "EntityType", "EntityId" });
        migrationBuilder.CreateIndex(name: "IX_MutationAuditLogs_ActorUserId", table: "MutationAuditLogs", column: "ActorUserId");
        migrationBuilder.CreateIndex(name: "IX_MutationAuditLogs_CreatedAt", table: "MutationAuditLogs", column: "CreatedAt");

        migrationBuilder.CreateTable(
            name: "Insureds",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<string>(type: "text", nullable: false),
                FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                BusinessName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                TaxId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                Address = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                AssignedAgentId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Insureds", x => x.Id);
                table.ForeignKey("FK_Insureds_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Insureds_Users_LinkedUserId", x => x.LinkedUserId, "Users", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_Insureds_Users_AssignedAgentId", x => x.AssignedAgentId, "Users", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_Insureds_TenantId_AssignedAgentId", "Insureds", new[] { "TenantId", "AssignedAgentId" });

        migrationBuilder.CreateTable(
            name: "Submissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                InsuredId = table.Column<Guid>(type: "uuid", nullable: false),
                SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                AssignedUnderwriterId = table.Column<Guid>(type: "uuid", nullable: true),
                LineOfBusiness = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                RiskData = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                DeclineReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Submissions", x => x.Id);
                table.ForeignKey("FK_Submissions_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Submissions_Insureds_InsuredId", x => x.InsuredId, "Insureds", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Submissions_Users_SubmittedByUserId", x => x.SubmittedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Submissions_Users_AssignedUnderwriterId", x => x.AssignedUnderwriterId, "Users", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_Submissions_TenantId_Status", "Submissions", new[] { "TenantId", "Status" });
        migrationBuilder.CreateIndex("IX_Submissions_InsuredId", "Submissions", "InsuredId");

        migrationBuilder.CreateTable(
            name: "Quotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                TotalPremium = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                Taxes = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                Fees = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                TotalDue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                QuoteExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                Coverages = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                Terms = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                BindRequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Quotes", x => x.Id);
                table.ForeignKey("FK_Quotes_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Quotes_Submissions_SubmissionId", x => x.SubmissionId, "Submissions", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Quotes_Users_IssuedByUserId", x => x.IssuedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_Quotes_SubmissionId", "Quotes", "SubmissionId");
        migrationBuilder.CreateIndex("IX_Quotes_Status_QuoteExpiryDate", "Quotes", new[] { "Status", "QuoteExpiryDate" });

        migrationBuilder.CreateTable(
            name: "Policies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                QuoteId = table.Column<Guid>(type: "uuid", nullable: true),
                InsuredId = table.Column<Guid>(type: "uuid", nullable: false),
                PolicyNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                LineOfBusiness = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                TotalPremium = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                AssignedAgentId = table.Column<Guid>(type: "uuid", nullable: true),
                CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CancellationReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                RenewalPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Policies", x => x.Id);
                table.ForeignKey("FK_Policies_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Policies_Quotes_QuoteId", x => x.QuoteId, "Quotes", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_Policies_Insureds_InsuredId", x => x.InsuredId, "Insureds", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Policies_Users_IssuedByUserId", x => x.IssuedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Policies_Users_AssignedAgentId", x => x.AssignedAgentId, "Users", "Id", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_Policies_Policies_RenewalPolicyId", x => x.RenewalPolicyId, "Policies", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_Policies_PolicyNumber", "Policies", "PolicyNumber", unique: true);
        migrationBuilder.CreateIndex("IX_Policies_TenantId_Status", "Policies", new[] { "TenantId", "Status" });

        migrationBuilder.CreateTable(
            name: "Coverages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                CoverageType = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                LimitPerOccurrence = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                LimitAggregate = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                Deductible = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                Premium = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Coverages", x => x.Id);
                table.ForeignKey("FK_Coverages_Policies_PolicyId", x => x.PolicyId, "Policies", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Coverages_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PolicyDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                BlobUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PolicyDocuments", x => x.Id);
                table.ForeignKey("FK_PolicyDocuments_Policies_PolicyId", x => x.PolicyId, "Policies", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_PolicyDocuments_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_PolicyDocuments_Users_UploadedByUserId", x => x.UploadedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Claims",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ClaimNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                ClaimantId = table.Column<Guid>(type: "uuid", nullable: false),
                ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Submitted"),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                ClaimedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                IncidentDate = table.Column<DateOnly>(type: "date", nullable: false),
                SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ReviewNotes = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Claims", x => x.Id);
                table.ForeignKey("FK_Claims_Policies_PolicyId", x => x.PolicyId, "Policies", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Claims_Users_ClaimantId", x => x.ClaimantId, "Users", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Claims_Users_ReviewedByUserId", x => x.ReviewedByUserId, "Users", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_Claims_ClaimNumber", "Claims", "ClaimNumber", unique: true);

        migrationBuilder.CreateTable(
            name: "RateTables",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                LineOfBusiness = table.Column<string>(type: "text", nullable: false),
                ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                TableVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                BaseRate = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                TaxRate = table.Column<decimal>(type: "numeric(8,6)", nullable: false),
                FlatFee = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                FactorsJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RateTables", x => x.Id);
                table.ForeignKey("FK_RateTables_Tenants_TenantId", x => x.TenantId, "Tenants", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_RateTables_TenantId_LineOfBusiness_ProductCode_TableVersion",
            "RateTables", new[] { "TenantId", "LineOfBusiness", "ProductCode", "TableVersion" }, unique: true);
        migrationBuilder.CreateIndex("IX_RateTables_TenantId_LineOfBusiness_IsActive",
            "RateTables", new[] { "TenantId", "LineOfBusiness", "IsActive" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("RateTables");
        migrationBuilder.DropTable("Claims");
        migrationBuilder.DropTable("PolicyDocuments");
        migrationBuilder.DropTable("Coverages");
        migrationBuilder.DropTable("Policies");
        migrationBuilder.DropTable("Quotes");
        migrationBuilder.DropTable("Submissions");
        migrationBuilder.DropTable("Insureds");
        migrationBuilder.DropTable("MutationAuditLogs");
        migrationBuilder.DropTable("AuditLogs");
        migrationBuilder.DropTable("PasswordResetTokens");
        migrationBuilder.DropTable("RefreshTokens");
        migrationBuilder.DropTable("Users");
        migrationBuilder.DropTable("Tenants");
    }
}
