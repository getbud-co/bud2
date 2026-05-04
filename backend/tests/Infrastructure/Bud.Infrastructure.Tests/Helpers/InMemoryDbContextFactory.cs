using Microsoft.Data.Sqlite;

namespace Bud.Infrastructure.Tests.Helpers;

internal static class SqliteDbContextFactory
{
    public static (ApplicationDbContext Context, SqliteConnection Connection) Create(ITenantProvider? tenantProvider = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var ctx = new ApplicationDbContext(options, tenantProvider);
        ctx.Database.EnsureCreated();
        return (ctx, connection);
    }

    public static ApplicationDbContext CreateWithConnection(SqliteConnection connection, ITenantProvider? tenantProvider = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options, tenantProvider);
    }
}
