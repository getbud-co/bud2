using Bud.Application.Common;
using Bud.Application.Features.Teams;
using Bud.Infrastructure.Features.Teams;
using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public sealed class TeamAuthorizationServiceTests
{
    private static ApplicationDbContext CreateInMemoryContext(TestTenantProvider tenantProvider)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, tenantProvider);
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenTeamExistsAndUserIsLeaderInTenant_ReturnsSuccess()
    {
        var teamId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var teamRepository = new Mock<ITeamRepository>();
        teamRepository
            .Setup(r => r.GetByIdAsync(teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team { Id = teamId, OrganizationId = organizationId });

        var tenantProvider = new TestTenantProvider { EmployeeId = employeeId };
        await using var dbContext = CreateInMemoryContext(tenantProvider);
        var emp = new Employee { Id = employeeId, FullName = "Leader", Email = "leader@test.com" };
        dbContext.Employees.Add(emp);
        dbContext.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember { EmployeeId = employeeId, OrganizationId = organizationId, Role = EmployeeRole.Leader });
        await dbContext.SaveChangesAsync();

        var service = new TeamAuthorizationService(teamRepository.Object, dbContext, tenantProvider);

        var result = await ((IWriteAuthorizationRule<TeamResource>)service)
            .EvaluateAsync(new TeamResource(teamId));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenCreateContextAndUserIsLeaderInTenant_ReturnsSuccess()
    {
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var teamRepository = new Mock<ITeamRepository>(MockBehavior.Strict);
        var tenantProvider = new TestTenantProvider { EmployeeId = employeeId };
        await using var dbContext = CreateInMemoryContext(tenantProvider);
        var emp = new Employee { Id = employeeId, FullName = "Leader", Email = "leader@test.com" };
        dbContext.Employees.Add(emp);
        dbContext.OrganizationEmployeeMembers.Add(new OrganizationEmployeeMember { EmployeeId = employeeId, OrganizationId = organizationId, Role = EmployeeRole.Leader });
        await dbContext.SaveChangesAsync();

        var service = new TeamAuthorizationService(teamRepository.Object, dbContext, tenantProvider);

        var result = await ((IWriteAuthorizationRule<CreateTeamContext>)service)
            .EvaluateAsync(new CreateTeamContext(organizationId));

        result.IsSuccess.Should().BeTrue();
    }
}
