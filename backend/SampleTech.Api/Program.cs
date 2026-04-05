using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SampleTech.Api.Authorization;
using SampleTech.Api.Data;
using SampleTech.Api.Middleware;
using SampleTech.Api.Models;
using SampleTech.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Logging ─────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ─── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ─── Authentication ───────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");

var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Optional OIDC — enabled when Oidc:Authority is set in configuration (e.g. via env var).
// This makes the platform enterprise SSO-ready without requiring a provider in Phase 1.
var oidcAuthority = builder.Configuration["Oidc:Authority"];
if (!string.IsNullOrWhiteSpace(oidcAuthority))
{
    authBuilder.AddOpenIdConnect("oidc", options =>
    {
        options.Authority = oidcAuthority;
        options.ClientId = builder.Configuration["Oidc:ClientId"]
            ?? throw new InvalidOperationException("Oidc:ClientId is required when Oidc:Authority is set.");
        options.ClientSecret = builder.Configuration["Oidc:ClientSecret"];
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });
}

// ─── Authorization / RBAC ─────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, p =>
        p.RequireRole(UserRole.Admin.ToString()));

    options.AddPolicy(Policies.UnderwriterOrAbove, p =>
        p.RequireRole(UserRole.Admin.ToString(), UserRole.Underwriter.ToString()));

    options.AddPolicy(Policies.AgentOrAbove, p =>
        p.RequireRole(UserRole.Admin.ToString(), UserRole.Agent.ToString(), UserRole.Broker.ToString()));

    options.AddPolicy(Policies.BrokerOrAbove, p =>
        p.RequireRole(UserRole.Admin.ToString(), UserRole.Broker.ToString()));

    options.AddPolicy(Policies.StaffOnly, p =>
        p.RequireRole(
            UserRole.Admin.ToString(),
            UserRole.Underwriter.ToString(),
            UserRole.Agent.ToString(),
            UserRole.Broker.ToString()));

    options.AddPolicy(Policies.AnyRole, p =>
        p.RequireRole(
            UserRole.Admin.ToString(),
            UserRole.Underwriter.ToString(),
            UserRole.Agent.ToString(),
            UserRole.Broker.ToString(),
            UserRole.Client.ToString()));
});

// ─── Application services ─────────────────────────────────────────────────────
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IMutationAuditService, MutationAuditService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInsuredService, InsuredService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IRatingEngine, RatingEngine>();
builder.Services.AddScoped<IRateTableService, RateTableService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IClaimService, ClaimService>();

// Document storage — local file system in dev, swap to S3 in production
builder.Services.AddScoped<IDocumentStorageService, LocalDocumentStorageService>();

// ─── Controllers + JSON ───────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ─── CORS — tighten per-environment in production ─────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                      ?? ["http://localhost:4200"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SampleTech Insurance API",
        Version = "v1",
        Description = "REST API for the SampleTech Insurance Platform"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Middleware pipeline ──────────────────────────────────────────────────────
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SampleTech Insurance API v1"));
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>(); // Must come after UseAuthentication

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", version = "0.1.0" }))
   .AllowAnonymous();

// ─── Dev-only: auto-migrate ───────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
