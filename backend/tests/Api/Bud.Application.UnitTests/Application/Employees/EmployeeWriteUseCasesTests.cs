using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Employees;

public sealed class EmployeeWriteUseCasesTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();

    [Fact]
    public async Task CreateEmployee_WithValidTeamId_AssignsEmployeeTeamWithoutPrimaryTeam()
    {
        var tenantId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        _tenantProvider.SetupGet(provider => provider.TenantId).Returns(tenantId);
        _employeeRepository
            .Setup(repository => repository.CountTeamsByIdsAndOrganizationAsync(
                It.Is<List<Guid>>(ids => ids.Count == 1 && ids[0] == teamId),
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var useCase = new CreateEmployee(
            _employeeRepository.Object,
            _tenantProvider.Object,
            NullLogger<CreateEmployee>.Instance);

        var result = await useCase.ExecuteAsync(new CreateEmployeeCommand(
            "User",
            "user@test.com",
            EmployeeRole.Contributor,
            teamId,
            null));

        result.IsSuccess.Should().BeTrue();
        result.Value!.TeamId.Should().BeNull();
        result.Value.Employee.EmployeeTeams.Should().ContainSingle(team => team.TeamId == teamId && team.EmployeeId == result.Value.EmployeeId);
        _employeeRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployee_WithUnknownTeamId_ReturnsNotFound()
    {
        var tenantId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        _tenantProvider.SetupGet(provider => provider.TenantId).Returns(tenantId);
        _employeeRepository
            .Setup(repository => repository.CountTeamsByIdsAndOrganizationAsync(
                It.Is<List<Guid>>(ids => ids.Count == 1 && ids[0] == teamId),
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var useCase = new CreateEmployee(
            _employeeRepository.Object,
            _tenantProvider.Object,
            NullLogger<CreateEmployee>.Instance);

        var result = await useCase.ExecuteAsync(new CreateEmployeeCommand(
            "User",
            "user@test.com",
            EmployeeRole.Contributor,
            teamId,
            null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Time não encontrado.");
        _employeeRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Employee>(), It.IsAny<OrganizationEmployeeMember>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _employeeRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeProfile_WhenEmployeeNotFound_ReturnsNotFound()
    {
        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationEmployeeMember?)null);

        var useCase = new PatchEmployee(
            _employeeRepository.Object,
            NullLogger<PatchEmployee>.Instance);

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), new PatchEmployeeCommand(
            "User",
            "user@test.com",
            "user test",
            EmployeeLanguage.Pt,
            EmployeeRole.Contributor,
            Guid.NewGuid(),
            default));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteEmployee_WhenAuthorized_Succeeds()
    {
        var employeeId = Guid.NewGuid();
        var member = new OrganizationEmployeeMember
        {
            EmployeeId = employeeId,
            OrganizationId = Guid.NewGuid(),
            Employee = new Employee { Id = employeeId, FullName = "User", Email = "user@test.com" },
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _employeeRepository
            .Setup(repository => repository.HasSubordinatesAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _employeeRepository
            .Setup(repository => repository.HasMissionsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new DeleteEmployee(
            _employeeRepository.Object,
            NullLogger<DeleteEmployee>.Instance);

        var result = await useCase.ExecuteAsync(member.EmployeeId);

        result.IsSuccess.Should().BeTrue();
        _employeeRepository.Verify(repository => repository.RemoveAsync(member, It.IsAny<CancellationToken>()), Times.Once);
        _employeeRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployee_WhenHasSubordinates_ReturnsConflict()
    {
        var employeeId = Guid.NewGuid();
        var member = new OrganizationEmployeeMember
        {
            EmployeeId = employeeId,
            OrganizationId = Guid.NewGuid(),
            Employee = new Employee { Id = employeeId, FullName = "User", Email = "user@test.com" },
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _employeeRepository
            .Setup(repository => repository.HasSubordinatesAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new DeleteEmployee(
            _employeeRepository.Object,
            NullLogger<DeleteEmployee>.Instance);

        var result = await useCase.ExecuteAsync(member.EmployeeId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task UpdateEmployeeTeams_WhenAuthorized_Succeeds()
    {
        var employeeId = Guid.NewGuid();
        var member = new OrganizationEmployeeMember
        {
            EmployeeId = employeeId,
            OrganizationId = Guid.NewGuid(),
            Employee = new Employee { Id = employeeId, FullName = "User", Email = "user@test.com" },
        };

        _employeeRepository
            .Setup(repository => repository.GetByIdWithEmployeeTeamsAsync(member.EmployeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        var useCase = new PatchEmployeeTeams(
            _employeeRepository.Object,
            NullLogger<PatchEmployeeTeams>.Instance);

        var result = await useCase.ExecuteAsync(member.EmployeeId, new PatchEmployeeTeamsCommand([]));

        result.IsSuccess.Should().BeTrue();
        _employeeRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
