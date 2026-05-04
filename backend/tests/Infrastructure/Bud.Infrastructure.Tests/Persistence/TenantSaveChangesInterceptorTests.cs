using Bud.Infrastructure.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace Bud.Infrastructure.Tests.Persistence;

public sealed class TenantSaveChangesInterceptorTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public TenantSaveChangesInterceptorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    private ApplicationDbContext CreateContextWithInterceptor(TestTenantProvider provider)
    {
        var interceptor = new TenantSaveChangesInterceptor(provider);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(interceptor)
            .Options;

        var ctx = new ApplicationDbContext(options, provider);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task SavingChanges_ShouldNotOverwriteExistingOrganizationId()
    {
        var tenantId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var provider = new TestTenantProvider { TenantId = tenantId };

        await using var ctx = CreateContextWithInterceptor(provider);

        var org = Organization.Create(orgId, OrganizationDomainName.Create("empresa.com"));
        ctx.Organizations.Add(org);

        var employee = Employee.Create(
            Guid.NewGuid(), orgId,
            EmployeeName.Create("João Silva"), EmailAddress.Create("joao@bud.co"),
            EmployeeRole.IndividualContributor);
        ctx.Employees.Add(employee);
        await ctx.SaveChangesAsync();

        employee.OrganizationId.Should().Be(orgId);
    }

    [Fact]
    public async Task SavingChanges_WithoutTenantId_ShouldNotModifyEntities()
    {
        var orgId = Guid.NewGuid();
        var provider = new TestTenantProvider { TenantId = null };

        await using var ctx = CreateContextWithInterceptor(provider);

        var org = Organization.Create(orgId, OrganizationDomainName.Create("empresa.com"));
        ctx.Organizations.Add(org);

        var employee = Employee.Create(
            Guid.NewGuid(), orgId,
            EmployeeName.Create("João Silva"), EmailAddress.Create("joao@bud.co"),
            EmployeeRole.IndividualContributor);
        ctx.Employees.Add(employee);
        await ctx.SaveChangesAsync();

        employee.OrganizationId.Should().Be(orgId);
    }

    public void Dispose() => _connection.Dispose();
}
