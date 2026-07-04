using Butterfly.Data;
using Microsoft.EntityFrameworkCore;

namespace Butterfly.Api.Tests.TestSupport;

/// <summary>Builds an isolated in-memory <see cref="ButterflyDbContext"/> per test.</summary>
public static class TestDb
{
    public static ButterflyDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ButterflyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new ButterflyDbContext(options);
    }
}
