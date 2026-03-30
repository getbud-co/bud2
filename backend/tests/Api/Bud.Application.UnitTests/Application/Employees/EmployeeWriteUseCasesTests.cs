using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Employees;

public sealed class EmployeeWriteUseCasesTests
{
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")]));

    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IApplicationAuthorizationGateway> _authorizationGateway = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();

    [Fact]
    public async Task CreateEmployee_WhenUnauthorized_ReturnsForbidden()
    {
        var tenantId = Guid.NewGuid();

        _tenantProvider.SetupGet(provider => provider.TenantId).Returns(tenantId);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<CreateEmployeeContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new CreateEmployee(
            _employeeRepository.Object,
            _authorizationGateway.Object,
            _tenantProvider.Object,
            NullLogger<CreateEmployee>.Instance);

        var result = await useCase.ExecuteAsync(User, new CreateEmployeeCommand(
            "User",
            "user@test.com",
            EmployeeRole.IndividualContributor,
            null,
            null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task UpdateEmployeeProfile_WhenEmployeeNotFound_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var useCase = new PatchEmployee(
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<PatchEmployee>.Instance);

        var result = await useCase.ExecuteAsync(User, Guid.NewGuid(), new PatchEmployeeCommand(
            "User",
            "user@test.com",
            EmployeeRole.IndividualContributor,
            default));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteEmployee_WhenAuthorized_Succeeds()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "User",
            Email = "user@test.com",
            OrganizationId = Guid.NewGuid()
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _employeeRepository
            .Setup(repository => repository.HasSubordinatesAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepository
            .Setup(repository => repository.HasMissionsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<EmployeeResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new DeleteEmployee(
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<DeleteEmployee>.Instance);

        var result = await useCase.ExecuteAsync(User, employee.Id);

        result.IsSuccess.Should().BeTrue();
        _employeeRepository.Verify(repository => repository.RemoveAsync(employee, It.IsAny<CancellationToken>()), Times.Once);
        _employeeRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployee_WhenHasSubordinates_ReturnsConflict()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "User",
            Email = "user@test.com",
            OrganizationId = Guid.NewGuid()
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _employeeRepository
            .Setup(repository => repository.HasSubordinatesAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<EmployeeResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new DeleteEmployee(
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<DeleteEmployee>.Instance);

        var result = await useCase.ExecuteAsync(User, employee.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task UpdateEmployeeTeams_WhenAuthorized_Succeeds()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "User",
            Email = "user@test.com",
            OrganizationId = Guid.NewGuid()
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdWithEmployeeTeamsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<EmployeeResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new PatchEmployeeTeams(
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<PatchEmployeeTeams>.Instance);

        var result = await useCase.ExecuteAsync(User, employee.Id, new PatchEmployeeTeamsCommand([]));

        result.IsSuccess.Should().BeTrue();
        _employeeRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployeeTeams_WhenUnauthorized_ReturnsForbidden()
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = "User",
            Email = "user@test.com",
            OrganizationId = Guid.NewGuid()
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdWithEmployeeTeamsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<EmployeeResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new PatchEmployeeTeams(
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<PatchEmployeeTeams>.Instance);

        var result = await useCase.ExecuteAsync(User, employee.Id, new PatchEmployeeTeamsCommand([]));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
    }
}
