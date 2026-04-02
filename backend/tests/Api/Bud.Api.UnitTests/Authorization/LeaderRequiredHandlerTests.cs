using System.Security.Claims;
using Bud.Api.Authorization.Handlers;
using Bud.Api.Authorization.Requirements;
using Bud.Api.UnitTests.Helpers;
using Bud.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Bud.Api.UnitTests.Authorization;

public sealed class LeaderRequiredHandlerTests
{
    private static ApplicationDbContext CreateContext(TestTenantProvider tenantProvider)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options, tenantProvider);
    }

    private static AuthorizationHandlerContext CreateContext(LeaderRequiredRequirement requirement) =>
        new([requirement], new ClaimsPrincipal(new ClaimsIdentity()), resource: null);

    [Fact]
    public async Task Handle_WhenGlobalAdmin_ShouldSucceed()
    {
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = true, TenantId = null, EmployeeId = null };
        using var dbContext = CreateContext(tenantProvider);
        var handler = new LeaderRequiredHandler(dbContext, tenantProvider);
        var requirement = new LeaderRequiredRequirement();
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenLeaderOfSameOrganization_ShouldSucceed()
    {
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = false, TenantId = orgId, EmployeeId = employeeId };
        using var dbContext = CreateContext(tenantProvider);

        var org = new Organization { Id = orgId, Name = "Test Org" };
        var employee = new Employee
        {
            Id = employeeId,
            OrganizationId = orgId,
            FullName = "Leader",
            Email = "leader@test.com",
            Role = EmployeeRole.Leader
        };
        dbContext.Organizations.Add(org);
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        var handler = new LeaderRequiredHandler(dbContext, tenantProvider);
        var requirement = new LeaderRequiredRequirement();
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenIndividualContributor_ShouldNotSucceed()
    {
        var orgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = false, TenantId = orgId, EmployeeId = employeeId };
        using var dbContext = CreateContext(tenantProvider);

        var org = new Organization { Id = orgId, Name = "Test Org" };
        var employee = new Employee
        {
            Id = employeeId,
            OrganizationId = orgId,
            FullName = "Contributor",
            Email = "contrib@test.com",
            Role = EmployeeRole.IndividualContributor
        };
        dbContext.Organizations.Add(org);
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        var handler = new LeaderRequiredHandler(dbContext, tenantProvider);
        var requirement = new LeaderRequiredRequirement();
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenLeaderOfDifferentOrganization_ShouldNotSucceed()
    {
        var orgId = Guid.NewGuid();
        var otherOrgId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = false, TenantId = orgId, EmployeeId = employeeId };
        using var dbContext = CreateContext(tenantProvider);

        var org = new Organization { Id = orgId, Name = "Test Org" };
        var otherOrg = new Organization { Id = otherOrgId, Name = "Other Org" };
        var employee = new Employee
        {
            Id = employeeId,
            OrganizationId = otherOrgId,
            FullName = "Leader Elsewhere",
            Email = "leader@other.com",
            Role = EmployeeRole.Leader
        };
        dbContext.Organizations.AddRange(org, otherOrg);
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        var handler = new LeaderRequiredHandler(dbContext, tenantProvider);
        var requirement = new LeaderRequiredRequirement();
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenEmployeeIdIsNull_ShouldNotSucceed()
    {
        var orgId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider { IsGlobalAdmin = false, TenantId = orgId, EmployeeId = null };
        using var dbContext = CreateContext(tenantProvider);

        var handler = new LeaderRequiredHandler(dbContext, tenantProvider);
        var requirement = new LeaderRequiredRequirement();
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeFalse();
    }
}
