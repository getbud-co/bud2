using Bud.Infrastructure.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace Bud.Infrastructure.Tests.Persistence;

public sealed class EfUnitOfWorkTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public EfUnitOfWorkTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var ctx = SqliteDbContextFactory.CreateWithConnection(_connection);
        ctx.Database.EnsureCreated();
    }

    [Fact]
    public async Task CommitAsync_ShouldPersistChangesInTransaction()
    {
        await using (var ctx = SqliteDbContextFactory.CreateWithConnection(_connection))
        {
            var unitOfWork = new EfUnitOfWork(ctx);
            ctx.Organizations.Add(Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create("empresa.com")));
            await unitOfWork.CommitAsync();
        }

        await using (var ctx = SqliteDbContextFactory.CreateWithConnection(_connection))
        {
            var org = await ctx.Organizations.IgnoreQueryFilters().FirstOrDefaultAsync();
            org.Should().NotBeNull();
            org!.Name.Value.Should().Be("empresa.com");
        }
    }

    public void Dispose() => _connection.Dispose();
}
