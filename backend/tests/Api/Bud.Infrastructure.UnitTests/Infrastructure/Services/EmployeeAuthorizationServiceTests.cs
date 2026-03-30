using Bud.Application.Common;
using Bud.Application.Features.Employees;
using Bud.Infrastructure.Features.Employees;
using Bud.Infrastructure.Persistence;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public sealed class EmployeeAuthorizationServiceTests
{
    private static ApplicationDbContext CreateInMemoryContext(TestTenantProvider tenantProvider)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, tenantProvider);
    }

    [Fact]
    public async Task EvaluateReadAsync_WhenTenantMatches_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var repository = new Mock<IEmployeeRepository>();
        repository
            .Setup(r => r.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = employeeId, OrganizationId = organizationId });

        var tenantProvider = new TestTenantProvider { TenantId = organizationId };
        await using var dbContext = CreateInMemoryContext(tenantProvider);
        var service = new EmployeeAuthorizationService(repository.Object, dbContext, tenantProvider);

        var result = await service.EvaluateAsync(new EmployeeResource(employeeId));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenCreateContextAndUserIsLeaderInTenant_ReturnsSuccess()
    {
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var repository = new Mock<IEmployeeRepository>(MockBehavior.Strict);
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

        var service = new EmployeeAuthorizationService(repository.Object, dbContext, tenantProvider);

        var result = await ((IWriteAuthorizationRule<CreateEmployeeContext>)service)
            .EvaluateAsync(new CreateEmployeeContext(organizationId));

        result.IsSuccess.Should().BeTrue();
    }
}
