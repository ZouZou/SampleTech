using FluentAssertions;
using SampleTech.Api.Models;
using SampleTech.Api.Services;
using SampleTech.Api.Tests.Helpers;
using Xunit;

namespace SampleTech.Api.Tests.Services;

public class AuditServiceTests : IDisposable
{
    private readonly SampleTech.Api.Data.AppDbContext _db;
    private readonly AuditService _sut;

    public AuditServiceTests()
    {
        _db = TestDbContextFactory.Create();
        _sut = new AuditService(_db);
    }

    [Fact]
    public async Task LogAsync_PersistsAuditLogWithAllFields()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var ctx = new AuditEventContext(
            AuthEventType.LoginSuccess,
            Email: "test@example.com",
            UserId: userId,
            TenantId: tenantId,
            IpAddress: "127.0.0.1",
            UserAgent: "TestAgent/1.0",
            Metadata: "{\"key\":\"value\"}");

        await _sut.LogAsync(ctx);

        var log = _db.AuditLogs.Single();
        log.EventType.Should().Be(AuthEventType.LoginSuccess);
        log.Email.Should().Be("test@example.com");
        log.UserId.Should().Be(userId);
        log.TenantId.Should().Be(tenantId);
        log.IpAddress.Should().Be("127.0.0.1");
        log.UserAgent.Should().Be("TestAgent/1.0");
        log.Metadata.Should().Be("{\"key\":\"value\"}");
        log.OccurredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LogAsync_LoginFailed_AllowsNullUserId()
    {
        var ctx = new AuditEventContext(
            AuthEventType.LoginFailed,
            Email: "unknown@example.com",
            UserId: null);

        await _sut.LogAsync(ctx);

        var log = _db.AuditLogs.Single();
        log.UserId.Should().BeNull();
        log.Email.Should().Be("unknown@example.com");
    }

    [Fact]
    public async Task LogAsync_MultipleCalls_PersistMultipleLogs()
    {
        await _sut.LogAsync(new AuditEventContext(AuthEventType.LoginSuccess));
        await _sut.LogAsync(new AuditEventContext(AuthEventType.Logout));
        await _sut.LogAsync(new AuditEventContext(AuthEventType.TokenRefreshed));

        _db.AuditLogs.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(AuthEventType.LoginSuccess)]
    [InlineData(AuthEventType.LoginFailed)]
    [InlineData(AuthEventType.Logout)]
    [InlineData(AuthEventType.TokenRefreshed)]
    [InlineData(AuthEventType.TokenRefreshFailed)]
    [InlineData(AuthEventType.AccountLocked)]
    public async Task LogAsync_AllEventTypes_AreStoredCorrectly(AuthEventType eventType)
    {
        await _sut.LogAsync(new AuditEventContext(eventType));

        _db.AuditLogs.Should().Contain(l => l.EventType == eventType);
    }

    public void Dispose() => _db.Dispose();
}
