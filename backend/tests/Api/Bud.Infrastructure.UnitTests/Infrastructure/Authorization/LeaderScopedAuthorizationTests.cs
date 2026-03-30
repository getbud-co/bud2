using Bud.Application.Common;
using Bud.Infrastructure.Authorization;
using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Authorization;

public sealed class LeaderScopedAuthorizationTests
{
    [Fact]
    public async Task RequireLeaderInOrganizationAsync_WhenEmployeeIsLeaderInOrganization_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider { EmployeeId = employeeId };
        await using var dbContext = CreateInMemoryContext(tenantProvider);
        dbContext.Employees.Add(new Employee
        {
            Id = employeeId,
            FullName = "Leader",
            Email = "leader@test.com",
            Role = EmployeeRole.Leader,
            OrganizationId = organizationId
        });
        await dbContext.SaveChangesAsync();

        var result = await LeaderScopedAuthorization.RequireLeaderInOrganizationAsync(
            dbContext,
            tenantProvider,
            organizationId,
            "Funcionário não identificado.",
            "negado");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RequireLeaderInOrganizationAsync_WhenEmployeeIsNotLeader_ReturnsForbidden()
    {
        var employeeId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider { EmployeeId = employeeId };
        await using var dbContext = CreateInMemoryContext(tenantProvider);
        dbContext.Employees.Add(new Employee
        {
            Id = employeeId,
            FullName = "Contributor",
            Email = "contributor@test.com",
            Role = EmployeeRole.IndividualContributor,
            OrganizationId = organizationId
        });
        await dbContext.SaveChangesAsync();

        var result = await LeaderScopedAuthorization.RequireLeaderInOrganizationAsync(
            dbContext,
            tenantProvider,
            organizationId,
            "Funcionário não identificado.",
            "negado");

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("negado");
    }

    private static ApplicationDbContext CreateInMemoryContext(TestTenantProvider tenantProvider)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, tenantProvider);
    }
}
