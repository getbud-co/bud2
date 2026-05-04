using Bud.Infrastructure.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace Bud.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests using SQLite InMemory. Methods that use EF.Property&lt;string&gt; with value converters
/// (IsEmailUniqueAsync, search queries) require Npgsql for integration testing.
/// </summary>
public sealed class EmployeeRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly Guid _tenantId = Guid.NewGuid();

    public EmployeeRepositoryTests()
    {
        var provider = new TestTenantProvider { TenantId = _tenantId };
        var (ctx, conn) = SqliteDbContextFactory.Create(provider);
        _connection = conn;

        ctx.Organizations.Add(Organization.Create(_tenantId, OrganizationDomainName.Create("empresa.com")));
        ctx.SaveChanges();
        ctx.Dispose();
    }

    private ApplicationDbContext CreateContext(Guid? tenantId = null) =>
        SqliteDbContextFactory.CreateWithConnection(_connection,
            new TestTenantProvider { TenantId = tenantId ?? _tenantId });

    private static Employee MakeEmployee(Guid orgId, string name = "João Silva", string email = "joao@bud.co") =>
        Employee.Create(Guid.NewGuid(), orgId, EmployeeName.Create(name), EmailAddress.Create(email), EmployeeRole.IndividualContributor);

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_ShouldPersistAndRetrieve()
    {
        var employee = MakeEmployee(_tenantId);

        await using (var ctx = CreateContext())
        {
            var repo = new EmployeeRepository(ctx);
            await repo.AddAsync(employee);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var repo = new EmployeeRepository(ctx);
            var found = await repo.GetByIdAsync(employee.Id);

            found.Should().NotBeNull();
            found!.FullName.Value.Should().Be("João Silva");
            found.Email.Value.Should().Be("joao@bud.co");
        }
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteEmployee()
    {
        var employee = MakeEmployee(_tenantId);

        await using (var ctx = CreateContext())
        {
            var repo = new EmployeeRepository(ctx);
            await repo.AddAsync(employee);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var repo = new EmployeeRepository(ctx);
            var found = await repo.GetByIdAsync(employee.Id);
            await repo.RemoveAsync(found!);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var repo = new EmployeeRepository(ctx);
            var found = await repo.GetByIdAsync(employee.Id);
            found.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResults()
    {
        await using (var ctx = CreateContext())
        {
            var repo = new EmployeeRepository(ctx);
            await repo.AddAsync(MakeEmployee(_tenantId, "Alice Costa", "alice@bud.co"));
            await repo.AddAsync(MakeEmployee(_tenantId, "Bruno Lima", "bruno@bud.co"));
            await repo.AddAsync(MakeEmployee(_tenantId, "Carlos Dias", "carlos@bud.co"));
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var repo = new EmployeeRepository(ctx);
            var result = await repo.GetAllAsync(null, 1, 2);

            result.Total.Should().Be(3);
            result.Items.Should().HaveCount(2);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(2);
        }
    }

    [Fact]
    public async Task GetByIdAsync_WithDifferentTenant_ShouldReturnNull()
    {
        var employee = MakeEmployee(_tenantId);

        await using (var ctx = CreateContext())
        {
            var repo = new EmployeeRepository(ctx);
            await repo.AddAsync(employee);
            await repo.SaveChangesAsync();
        }

        var otherTenant = Guid.NewGuid();
        await using (var ctx = CreateContext(otherTenant))
        {
            var repo = new EmployeeRepository(ctx);
            var found = await repo.GetByIdAsync(employee.Id);
            found.Should().BeNull();
        }
    }

    public void Dispose() => _connection.Dispose();
}
