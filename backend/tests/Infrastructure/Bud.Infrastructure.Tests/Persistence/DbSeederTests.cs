using Bud.Infrastructure.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace Bud.Infrastructure.Tests.Persistence;

/// <summary>
/// DbSeeder uses EF.Property&lt;string&gt; for email/name lookup which requires Npgsql.
/// Tests here cover the validation edge cases that don't hit the database queries.
/// </summary>
public sealed class DbSeederTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public DbSeederTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var ctx = SqliteDbContextFactory.CreateWithConnection(_connection);
        ctx.Database.EnsureCreated();
    }

    private ApplicationDbContext CreateContext() =>
        SqliteDbContextFactory.CreateWithConnection(_connection);

    private static IOptions<GlobalAdminSettings> CreateSettings(string orgName = "admin.bud.local", string email = "admin@bud.co") =>
        Options.Create(new GlobalAdminSettings { OrganizationName = orgName, Email = email });

    [Fact]
    public async Task SeedAsync_WithInvalidOrgName_ShouldDoNothing()
    {
        await using var ctx = CreateContext();
        var settings = CreateSettings(orgName: "not a domain");

        await DbSeeder.SeedAsync(ctx, settings);

        var orgs = await ctx.Organizations.IgnoreQueryFilters().CountAsync();
        orgs.Should().Be(0);
    }

    [Fact]
    public async Task SeedAsync_WithInvalidEmail_ShouldDoNothing()
    {
        await using var ctx = CreateContext();
        var settings = CreateSettings(email: "invalid");

        await DbSeeder.SeedAsync(ctx, settings);

        var orgs = await ctx.Organizations.IgnoreQueryFilters().CountAsync();
        orgs.Should().Be(0);
    }

    [Fact]
    public async Task SeedAsync_WithEmptyOrgName_ShouldDoNothing()
    {
        await using var ctx = CreateContext();
        var settings = CreateSettings(orgName: "");

        await DbSeeder.SeedAsync(ctx, settings);

        var orgs = await ctx.Organizations.IgnoreQueryFilters().CountAsync();
        orgs.Should().Be(0);
    }

    public void Dispose() => _connection.Dispose();
}
