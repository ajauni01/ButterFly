using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Butterfly.Data;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can construct the context from this class library
/// without the API host. The connection string here is a placeholder — EF does not connect to a
/// database to scaffold or script migrations. The runtime connection string comes from the API's
/// configuration (user-secrets in dev, Key Vault/managed identity in prod).
/// </summary>
public class ButterflyDbContextFactory : IDesignTimeDbContextFactory<ButterflyDbContext>
{
    public ButterflyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ButterflyDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Butterfly;Trusted_Connection=True;")
            .Options;
        return new ButterflyDbContext(options);
    }
}
