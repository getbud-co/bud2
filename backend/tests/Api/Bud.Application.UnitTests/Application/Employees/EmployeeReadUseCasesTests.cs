using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Employees;

public sealed class EmployeeReadUseCasesTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IApplicationAuthorizationGateway> _authorizationGateway = new();

    [Fact]
    public async Task GetEmployeeById_WithExistingEmployee_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee
            {
                Id = employeeId,
                FullName = "Ana",
                Email = "ana@getbud.co",
                OrganizationId = Guid.NewGuid()
            });

        _authorizationGateway
            .Setup(gateway => gateway.CanReadAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<EmployeeResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new GetEmployeeById(_employeeRepository.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(new ClaimsIdentity()), employeeId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(employeeId);
    }

    [Fact]
    public async Task GetEmployeeById_WithNonExistingId_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var useCase = new GetEmployeeById(_employeeRepository.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(new ClaimsIdentity()), Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ListLeaders_ReturnsSuccess()
    {
        var organizationId = Guid.NewGuid();

        _employeeRepository
            .Setup(repository => repository.GetLeadersAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var useCase = new ListLeaderEmployees(_employeeRepository.Object);

        var result = await useCase.ExecuteAsync(organizationId);

        result.IsSuccess.Should().BeTrue();
        _employeeRepository.Verify(repository => repository.GetLeadersAsync(organizationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListAvailableEmployeeTeams_WithExistingEmployee_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee
            {
                Id = employeeId,
                FullName = "Ana",
                Email = "ana@getbud.co",
                OrganizationId = organizationId
            });
        _employeeRepository
            .Setup(repository => repository.GetEligibleTeamsForAssignmentAsync(employeeId, organizationId, "produto", 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _authorizationGateway
            .Setup(gateway => gateway.CanReadAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<EmployeeResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new ListAvailableTeamsForEmployee(_employeeRepository.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(new ClaimsIdentity()), employeeId, "produto");

        result.IsSuccess.Should().BeTrue();
        _employeeRepository.Verify(
            repository => repository.GetEligibleTeamsForAssignmentAsync(employeeId, organizationId, "produto", 50, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetEmployeeHierarchy_WithNonExistingEmployee_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(repository => repository.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new GetEmployeeHierarchy(_employeeRepository.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(new ClaimsIdentity()), Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
