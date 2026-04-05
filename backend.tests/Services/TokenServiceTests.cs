using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using JwtClaim = System.Security.Claims.Claim;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SampleTech.Api.Models;
using SampleTech.Api.Services;
using SampleTech.Api.Tests.Helpers;
using Xunit;

namespace SampleTech.Api.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly string _signingKey = "SuperSecretTestKeyThatIsLongEnoughForHS256!";

    public TokenServiceTests()
    {
        var config = ConfigurationFactory.CreateJwtConfig(
            key: _signingKey,
            issuer: "test-issuer",
            audience: "test-audience",
            accessExpiryMinutes: 30);

        _sut = new TokenService(config);
    }

    [Fact]
    public void GenerateTokenPair_ReturnsNonEmptyTokens()
    {
        var user = BuildUser();

        var pair = _sut.GenerateTokenPair(user);

        pair.AccessToken.Should().NotBeNullOrWhiteSpace();
        pair.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateTokenPair_AccessToken_ContainsSubClaim()
    {
        var user = BuildUser();

        var pair = _sut.GenerateTokenPair(user);
        var claims = ParseClaims(pair.AccessToken);

        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateTokenPair_AccessToken_ContainsEmailClaim()
    {
        var user = BuildUser();

        var pair = _sut.GenerateTokenPair(user);
        var claims = ParseClaims(pair.AccessToken);

        claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
    }

    [Fact]
    public void GenerateTokenPair_AccessToken_ContainsRoleClaim()
    {
        var user = BuildUser(role: UserRole.Underwriter);

        var pair = _sut.GenerateTokenPair(user);
        var claims = ParseClaims(pair.AccessToken);

        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Underwriter");
    }

    [Fact]
    public void GenerateTokenPair_AccessToken_ContainsTenantIdClaim_WhenTenantIsSet()
    {
        var tenantId = Guid.NewGuid();
        var user = BuildUser(tenantId: tenantId);

        var pair = _sut.GenerateTokenPair(user);
        var claims = ParseClaims(pair.AccessToken);

        claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
    }

    [Fact]
    public void GenerateTokenPair_AccessToken_DoesNotContainTenantIdClaim_WhenNoTenant()
    {
        var user = BuildUser(withTenant: false);

        var pair = _sut.GenerateTokenPair(user);
        var claims = ParseClaims(pair.AccessToken);

        claims.Should().NotContain(c => c.Type == "tenant_id");
    }

    [Fact]
    public void GenerateTokenPair_AccessTokenExpiry_MatchesConfiguredMinutes()
    {
        var user = BuildUser();
        var before = DateTimeOffset.UtcNow;

        var pair = _sut.GenerateTokenPair(user);

        pair.AccessTokenExpiry.Should().BeCloseTo(before.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateTokenPair_RefreshToken_IsDifferentEachCall()
    {
        var user = BuildUser();

        var pair1 = _sut.GenerateTokenPair(user);
        var pair2 = _sut.GenerateTokenPair(user);

        pair1.RefreshToken.Should().NotBe(pair2.RefreshToken);
    }

    [Fact]
    public void GenerateTokenPair_AccessToken_IsSignedWithConfiguredKey()
    {
        var user = BuildUser();

        var pair = _sut.GenerateTokenPair(user);

        var handler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "test-issuer",
            ValidateAudience = true,
            ValidAudience = "test-audience",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey)),
            ClockSkew = TimeSpan.Zero
        };

        var act = () => handler.ValidateToken(pair.AccessToken, validationParams, out _);
        act.Should().NotThrow();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static User BuildUser(
        UserRole role = UserRole.Agent,
        Guid? tenantId = null,
        bool withTenant = true) => new()
    {
        Id = Guid.NewGuid(),
        Email = "test@example.com",
        FirstName = "Test",
        LastName = "User",
        Role = role,
        TenantId = withTenant ? (tenantId ?? Guid.NewGuid()) : null
    };

    private IEnumerable<JwtClaim> ParseClaims(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        return jwt.Claims;
    }
}
