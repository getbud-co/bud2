using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Employees;

public sealed class EmployeeReadUseCasesTests
{
    private readonly Mock<IMemberRepository> _employeeRepository = new();

    [Fact]
    public async Task GetEmployeeById_WithExistingEmployee_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var member = new OrganizationEmployeeMember
        {
            EmployeeId = employeeId,
            OrganizationId = Guid.NewGuid(),
            Employee = new Employee { Id = employeeId, FullName = "Ana", Email = "ana@getbud.co" },
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var useCase = new GetEmployeeById(_employeeRepository.Object);

        var result = await useCase.ExecuteAsync(employeeId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public async Task GetEmployeeById_WithNonExistingId_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationEmployeeMember?)null);

        var useCase = new GetEmployeeById(_employeeRepository.Object);

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ListLeaders_ReturnsSuccess()
    {
        _employeeRepository
            .Setup(repository => repository.GetLeadersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var useCase = new ListLeaderEmployees(_employeeRepository.Object);

        var result = await useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        _employeeRepository.Verify(repository => repository.GetLeadersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListAvailableEmployeeTeams_WithExistingEmployee_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var member = new OrganizationEmployeeMember
        {
            EmployeeId = employeeId,
            OrganizationId = organizationId,
            Employee = new Employee { Id = employeeId, FullName = "Ana", Email = "ana@getbud.co" },
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _employeeRepository
            .Setup(repository => repository.GetEligibleTeamsForAssignmentAsync(employeeId, organizationId, "produto", 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var useCase = new ListAvailableTeamsForEmployee(_employeeRepository.Object);

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
