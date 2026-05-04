using Bud.Infrastructure.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace Bud.Infrastructure.Tests.Sessions;

/// <summary>
/// TenantAuthorizationService uses EF.Property&lt;string&gt; with value-converted properties
/// for email lookup. Most tests require Npgsql. Tests here cover edge cases that don't
/// touch value-converted queries.
/// </summary>
public sealed class TenantAuthorizationServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public TenantAuthorizationServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var ctx = SqliteDbContextFactory.CreateWithConnection(_connection);
        ctx.Database.EnsureCreated();
    }

    [Fact]
    public async Task UserBelongsToTenantAsync_WithInvalidEmail_ShouldReturnFalse()
    {
        await using var ctx = SqliteDbContextFactory.CreateWithConnection(_connection);
        var provider = new TestTenantProvider { UserEmail = "invalid" };
        var sut = new TenantAuthorizationService(ctx, provider);

        var result = await sut.UserBelongsToTenantAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserTenantIdsAsync_WithNoEmail_ShouldReturnEmpty()
    {
        await using var ctx = SqliteDbContextFactory.CreateWithConnection(_connection);
        var provider = new TestTenantProvider { UserEmail = null };
        var sut = new TenantAuthorizationService(ctx, provider);

        var result = await sut.GetUserTenantIdsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserTenantIdsAsync_WithNullEmail_ShouldReturnEmpty()
    {
        await using var ctx = SqliteDbContextFactory.CreateWithConnection(_connection);
        var provider = new TestTenantProvider { UserEmail = "   " };
        var sut = new TenantAuthorizationService(ctx, provider);

        var result = await sut.GetUserTenantIdsAsync();

        result.Should().BeEmpty();
    }

    public void Dispose() => _connection.Dispose();
}
