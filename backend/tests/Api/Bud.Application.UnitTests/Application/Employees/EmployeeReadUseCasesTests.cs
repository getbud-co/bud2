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
    private readonly Mock<ITenantProvider> _tenantProvider = new();

    [Fact]
    public async Task GetEmployeeById_WithExistingEmployee_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var employee = new Employee { Id = employeeId, FullName = "Ana", Email = "ana@getbud.co" };

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var useCase = new GetEmployeeById(_employeeRepository.Object);

        var result = await useCase.ExecuteAsync(employeeId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(employeeId);
    }

    [Fact]
    public async Task GetEmployeeById_WithNonExistingId_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var useCase = new GetEmployeeById(_employeeRepository.Object);

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ListLeaders_ReturnsSuccess()
    {
        _employeeRepository
            .Setup(repository => repository.GetLeadersAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var useCase = new ListLeaderEmployees(_employeeRepository.Object);

        var result = await useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        _employeeRepository.Verify(repository => repository.GetLeadersAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListAvailableEmployeeTeams_WithExistingEmployee_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var employee = new Employee { Id = employeeId, FullName = "Ana", Email = "ana@getbud.co" };

        _tenantProvider.SetupGet(p => p.TenantId).Returns(organizationId);
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _employeeRepository
            .Setup(repository => repository.GetEligibleTeamsForAssignmentAsync(employeeId, organizationId, "produto", 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var useCase = new ListAvailableTeamsForEmployee(_employeeRepository.Object, _tenantProvider.Object);

        var result = await useCase.ExecuteAsync(employeeId, "produto");

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

        var useCase = new GetEmployeeHierarchy(_employeeRepository.Object);

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
