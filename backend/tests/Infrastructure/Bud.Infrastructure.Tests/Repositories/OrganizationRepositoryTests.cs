using Bud.Infrastructure.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace Bud.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests using SQLite InMemory. Methods that use EF.Property&lt;string&gt; with value converters
/// (ExistsByNameAsync) require Npgsql for integration testing.
/// </summary>
public sealed class OrganizationRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public OrganizationRepositoryTests()
    {
        var (ctx, conn) = SqliteDbContextFactory.Create(new TestTenantProvider { IsGlobalAdmin = true });
        _connection = conn;
        ctx.Dispose();
    }

    private ApplicationDbContext CreateContext(Guid? tenantId = null, bool isGlobalAdmin = false) =>
        SqliteDbContextFactory.CreateWithConnection(_connection,
            new TestTenantProvider { TenantId = tenantId, IsGlobalAdmin = isGlobalAdmin });

    private static Organization MakeOrg(string domain = "empresa.com") =>
        Organization.Create(Guid.NewGuid(), OrganizationDomainName.Create(domain));

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_ShouldPersistAndRetrieve()
    {
        var org = MakeOrg();

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            await repo.AddAsync(org);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            var found = await repo.GetByIdAsync(org.Id);

            found.Should().NotBeNull();
            found!.Name.Value.Should().Be("empresa.com");
        }
    }

    [Fact]
    public async Task HasEmployeesAsync_WhenNoEmployees_ShouldReturnFalse()
    {
        var org = MakeOrg();

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            await repo.AddAsync(org);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            var has = await repo.HasEmployeesAsync(org.Id);
            has.Should().BeFalse();
        }
    }

    [Fact]
    public async Task HasEmployeesAsync_WhenHasEmployees_ShouldReturnTrue()
    {
        var org = MakeOrg();
        var employee = Employee.Create(Guid.NewGuid(), org.Id,
            EmployeeName.Create("João"), EmailAddress.Create("joao@bud.co"), EmployeeRole.IndividualContributor);

        await using (var ctx = CreateContext(org.Id))
        {
            ctx.Organizations.Add(org);
            ctx.Employees.Add(employee);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            var has = await repo.HasEmployeesAsync(org.Id);
            has.Should().BeTrue();
        }
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteOrganization()
    {
        var org = MakeOrg();

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            await repo.AddAsync(org);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            var found = await repo.GetByIdAsync(org.Id);
            await repo.RemoveAsync(found!);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            var found = await repo.GetByIdAsync(org.Id);
            found.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResults()
    {
        await using (var ctx = CreateContext(isGlobalAdmin: true))
        {
            ctx.Organizations.Add(MakeOrg("alpha.com"));
            ctx.Organizations.Add(MakeOrg("beta.com"));
            ctx.Organizations.Add(MakeOrg("gamma.com"));
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = CreateContext(isGlobalAdmin: true))
        {
            var repo = new OrganizationRepository(ctx);
            var result = await repo.GetAllAsync(null, 1, 2);

            result.Total.Should().Be(3);
            result.Items.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task ExistsAsync_WhenOrganizationExists_ShouldReturnTrue()
    {
        var org = MakeOrg();

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            await repo.AddAsync(org);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext(org.Id))
        {
            var repo = new OrganizationRepository(ctx);
            var exists = await repo.ExistsAsync(org.Id);
            exists.Should().BeTrue();
        }
    }

    [Fact]
    public async Task ExistsAsync_WhenOrganizationDoesNotExist_ShouldReturnFalse()
    {
        await using var ctx = CreateContext(isGlobalAdmin: true);
        var repo = new OrganizationRepository(ctx);
        var exists = await repo.ExistsAsync(Guid.NewGuid());
        exists.Should().BeFalse();
    }

    public void Dispose() => _connection.Dispose();
}
