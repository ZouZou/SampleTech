using Microsoft.EntityFrameworkCore;
using SampleTech.Api.Data;

namespace SampleTech.Api.Tests.Helpers;

/// <summary>Creates an isolated in-memory AppDbContext for each test.</summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
