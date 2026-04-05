using FluentAssertions;
using Moq;
using SampleTech.Api.Data;
using SampleTech.Api.Models;
using SampleTech.Api.Services;
using SampleTech.Api.Tests.Helpers;
using Xunit;

namespace SampleTech.Api.Tests.Services;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly UserService _sut;
    private readonly Mock<IAuditService> _auditMock;
    private readonly ITokenService _tokenService;

    public UserServiceTests()
    {
        _db = TestDbContextFactory.Create();
        _auditMock = new Mock<IAuditService>();
        _auditMock.Setup(a => a.LogAsync(It.IsAny<AuditEventContext>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        var config = ConfigurationFactory.CreateJwtConfig();
        _tokenService = new TokenService(config);
        _sut = new UserService(_db, _tokenService, config, _auditMock.Object);
    }

    // ── LoginAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResult()
    {
        var user = await SeedUserAsync("alice@test.com", "Password1!", UserRole.Agent);

        var result = await _sut.LoginAsync("alice@test.com", "Password1!");

        result.Should().NotBeNull();
        result!.User.Email.Should().Be("alice@test.com");
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_StoresRefreshTokenInDb()
    {
        var user = await SeedUserAsync("bob@test.com", "Password1!", UserRole.Client);

        var result = await _sut.LoginAsync("bob@test.com", "Password1!");

        _db.RefreshTokens.Should().Contain(r => r.Token == result!.RefreshToken && r.UserId == user.Id);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_LogsLoginSuccessAuditEvent()
    {
        await SeedUserAsync("carol@test.com", "Password1!", UserRole.Broker);

        await _sut.LoginAsync("carol@test.com", "Password1!");

        _auditMock.Verify(a => a.LogAsync(
            It.Is<AuditEventContext>(ctx => ctx.EventType == AuthEventType.LoginSuccess),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        await SeedUserAsync("dave@test.com", "Password1!", UserRole.Underwriter);

        var result = await _sut.LoginAsync("dave@test.com", "WrongPassword!");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_LogsLoginFailedAuditEvent()
    {
        await SeedUserAsync("eve@test.com", "Password1!", UserRole.Agent);

        await _sut.LoginAsync("eve@test.com", "WrongPassword!");

        _auditMock.Verify(a => a.LogAsync(
            It.Is<AuditEventContext>(ctx => ctx.EventType == AuthEventType.LoginFailed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsNull()
    {
        var result = await _sut.LoginAsync("nobody@test.com", "Password1!");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_LogsLoginFailedAuditEvent()
    {
        await _sut.LoginAsync("nobody@test.com", "Password1!");

        _auditMock.Verify(a => a.LogAsync(
            It.Is<AuditEventContext>(ctx => ctx.EventType == AuthEventType.LoginFailed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        await SeedUserAsync("inactive@test.com", "Password1!", UserRole.Client, isActive: false);

        var result = await _sut.LoginAsync("inactive@test.com", "Password1!");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_EmailIsCaseInsensitive()
    {
        await SeedUserAsync("frank@test.com", "Password1!", UserRole.Broker);

        var result = await _sut.LoginAsync("FRANK@TEST.COM", "Password1!");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_IncludesIpAndUserAgentInAuditEvent()
    {
        await SeedUserAsync("grace@test.com", "Password1!", UserRole.Agent);

        await _sut.LoginAsync("grace@test.com", "Password1!", ipAddress: "1.2.3.4", userAgent: "TestBrowser/1.0");

        _auditMock.Verify(a => a.LogAsync(
            It.Is<AuditEventContext>(ctx =>
                ctx.EventType == AuthEventType.LoginSuccess &&
                ctx.IpAddress == "1.2.3.4" &&
                ctx.UserAgent == "TestBrowser/1.0"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── RefreshAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokenPair()
    {
        var user = await SeedUserAsync("henry@test.com", "Password1!", UserRole.Agent);
        var loginResult = await _sut.LoginAsync("henry@test.com", "Password1!");

        var refreshResult = await _sut.RefreshAsync(loginResult!.RefreshToken);

        refreshResult.Should().NotBeNull();
        refreshResult!.AccessToken.Should().NotBe(loginResult.AccessToken);
        refreshResult.RefreshToken.Should().NotBe(loginResult.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_RevokesOldToken()
    {
        await SeedUserAsync("ida@test.com", "Password1!", UserRole.Client);
        var loginResult = await _sut.LoginAsync("ida@test.com", "Password1!");
        var oldToken = loginResult!.RefreshToken;

        await _sut.RefreshAsync(oldToken);

        _db.RefreshTokens.Should().Contain(r => r.Token == oldToken && r.IsRevoked);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_LogsTokenRefreshedAuditEvent()
    {
        await SeedUserAsync("jack@test.com", "Password1!", UserRole.Broker);
        var loginResult = await _sut.LoginAsync("jack@test.com", "Password1!");
        _auditMock.Invocations.Clear();

        await _sut.RefreshAsync(loginResult!.RefreshToken);

        _auditMock.Verify(a => a.LogAsync(
            It.Is<AuditEventContext>(ctx => ctx.EventType == AuthEventType.TokenRefreshed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_UnknownToken_ReturnsNull()
    {
        var result = await _sut.RefreshAsync("unknown-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_UnknownToken_LogsTokenRefreshFailedAuditEvent()
    {
        await _sut.RefreshAsync("unknown-token");

        _auditMock.Verify(a => a.LogAsync(
            It.Is<AuditEventContext>(ctx => ctx.EventType == AuthEventType.TokenRefreshFailed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_RevokedToken_ReturnsNull()
    {
        await SeedUserAsync("karen@test.com", "Password1!", UserRole.Agent);
        var loginResult = await _sut.LoginAsync("karen@test.com", "Password1!");
        // Use it once (rotates and revokes)
        await _sut.RefreshAsync(loginResult!.RefreshToken);

        // Try to use the same (now revoked) token again
        var secondResult = await _sut.RefreshAsync(loginResult.RefreshToken);

        secondResult.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsNull()
    {
        var user = await SeedUserAsync("liam@test.com", "Password1!", UserRole.Client);
        // Insert a pre-expired refresh token directly
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "expired-token-abc",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1),
            IsRevoked = false
        });
        await _db.SaveChangesAsync();

        var result = await _sut.RefreshAsync("expired-token-abc");

        result.Should().BeNull();
    }

    // ── RevokeRefreshTokenAsync ───────────────────────────────────────────────

    [Fact]
    public async Task RevokeRefreshTokenAsync_ValidToken_RevokesIt()
    {
        await SeedUserAsync("mia@test.com", "Password1!", UserRole.Agent);
        var loginResult = await _sut.LoginAsync("mia@test.com", "Password1!");
        var token = loginResult!.RefreshToken;

        await _sut.RevokeRefreshTokenAsync(token);

        _db.RefreshTokens.Should().Contain(r => r.Token == token && r.IsRevoked);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ValidToken_LogsLogoutAuditEvent()
    {
        await SeedUserAsync("noah@test.com", "Password1!", UserRole.Broker);
        var loginResult = await _sut.LoginAsync("noah@test.com", "Password1!");
        _auditMock.Invocations.Clear();

        await _sut.RevokeRefreshTokenAsync(loginResult!.RefreshToken);

        _auditMock.Verify(a => a.LogAsync(
            It.Is<AuditEventContext>(ctx => ctx.EventType == AuthEventType.Logout),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_UnknownToken_DoesNotThrow()
    {
        var act = async () => await _sut.RevokeRefreshTokenAsync("nonexistent-token");

        await act.Should().NotThrowAsync();
    }

    // ── TenantId propagation ──────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_UserWithTenant_ReturnsTenantIdInDto()
    {
        var tenantId = Guid.NewGuid();
        await SeedUserAsync("olivia@test.com", "Password1!", UserRole.Agent, tenantId: tenantId);

        var result = await _sut.LoginAsync("olivia@test.com", "Password1!");

        result!.User.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task LoginAsync_AdminUserWithoutTenant_ReturnsNullTenantIdInDto()
    {
        await SeedUserAsync("peter@test.com", "Password1!", UserRole.Admin, tenantId: null);

        var result = await _sut.LoginAsync("peter@test.com", "Password1!");

        result!.User.TenantId.Should().BeNull();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<User> SeedUserAsync(
        string email,
        string password,
        UserRole role,
        bool isActive = true,
        Guid? tenantId = null)
    {
        var user = new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = "Test",
            LastName = "User",
            Role = role,
            Status = isActive ? UserStatus.Active : UserStatus.Deactivated,
            TenantId = tenantId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public void Dispose() => _db.Dispose();
}
