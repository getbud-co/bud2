using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Teams;

public sealed class TeamWriteUseCasesTests
{
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")]));

    private readonly Mock<ITeamRepository> _teamRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();
    private readonly Mock<IApplicationAuthorizationGateway> _authorizationGateway = new();

    [Fact]
    public async Task CreateTeam_WhenTenantNotSelected_ReturnsForbidden()
    {
        _tenantProvider.SetupGet(provider => provider.TenantId).Returns((Guid?)null);

        var useCase = new CreateTeam(
            _teamRepository.Object,
            _employeeRepository.Object,
            _tenantProvider.Object,
            _authorizationGateway.Object,
            NullLogger<CreateTeam>.Instance);

        var result = await useCase.ExecuteAsync(User, new CreateTeamCommand("Team", Guid.NewGuid(), null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Apenas um líder da organização pode criar times.");
    }

    [Fact]
    public async Task CreateTeam_WhenUnauthorized_ReturnsForbidden()
    {
        var organizationId = Guid.NewGuid();

        _tenantProvider.SetupGet(provider => provider.TenantId).Returns(organizationId);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<CreateTeamContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new CreateTeam(
            _teamRepository.Object,
            _employeeRepository.Object,
            _tenantProvider.Object,
            _authorizationGateway.Object,
            NullLogger<CreateTeam>.Instance);

        var result = await useCase.ExecuteAsync(User, new CreateTeamCommand("Team", Guid.NewGuid(), null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task UpdateTeam_WhenTeamNotFound_ReturnsNotFound()
    {
        _teamRepository
            .Setup(repository => repository.GetByIdWithEmployeeTeamsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var useCase = new PatchTeam(
            _teamRepository.Object,
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<PatchTeam>.Instance);

        var result = await useCase.ExecuteAsync(User, Guid.NewGuid(), new PatchTeamCommand("Novo Team", Guid.NewGuid(), default));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateTeam_WhenUnauthorized_ReturnsForbidden()
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Team",
            OrganizationId = Guid.NewGuid()
        };

        _teamRepository
            .Setup(repository => repository.GetByIdWithEmployeeTeamsAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<TeamResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new PatchTeam(
            _teamRepository.Object,
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<PatchTeam>.Instance);

        var result = await useCase.ExecuteAsync(User, team.Id, new PatchTeamCommand("Novo Team", Guid.NewGuid(), default));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task RemoveTeam_WhenAuthorized_Succeeds()
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Team",
            OrganizationId = Guid.NewGuid()
        };

        _teamRepository.Setup(repository => repository.GetByIdAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(team);
        _teamRepository.Setup(repository => repository.HasSubTeamsAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _teamRepository.Setup(repository => repository.HasMissionsAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<TeamResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new DeleteTeam(_teamRepository.Object, _authorizationGateway.Object, NullLogger<DeleteTeam>.Instance);

        var result = await useCase.ExecuteAsync(User, team.Id);

        result.IsSuccess.Should().BeTrue();
        _teamRepository.Verify(repository => repository.RemoveAsync(team, It.IsAny<CancellationToken>()), Times.Once);
        _teamRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveTeam_WithSubTeams_ReturnsConflict()
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Team",
            OrganizationId = Guid.NewGuid()
        };

        _teamRepository.Setup(repository => repository.GetByIdAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(team);
        _teamRepository.Setup(repository => repository.HasSubTeamsAsync(team.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<TeamResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new DeleteTeam(_teamRepository.Object, _authorizationGateway.Object, NullLogger<DeleteTeam>.Instance);

        var result = await useCase.ExecuteAsync(User, team.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task UpdateTeamEmployees_WhenAuthorized_Succeeds()
    {
        var leaderId = Guid.NewGuid();
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Team",
            OrganizationId = Guid.NewGuid(),
            LeaderId = leaderId
        };

        _teamRepository
            .Setup(repository => repository.GetByIdWithEmployeeTeamsAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<TeamResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _employeeRepository
            .Setup(repository => repository.CountByIdsAndOrganizationAsync(It.IsAny<List<Guid>>(), team.OrganizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var useCase = new PatchTeamEmployees(
            _teamRepository.Object,
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<PatchTeamEmployees>.Instance);

        var result = await useCase.ExecuteAsync(User, team.Id, new PatchTeamEmployeesCommand([leaderId]));

        result.IsSuccess.Should().BeTrue();
        _teamRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamEmployees_WhenUnauthorized_ReturnsForbidden()
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Team",
            OrganizationId = Guid.NewGuid()
        };

        _teamRepository
            .Setup(repository => repository.GetByIdWithEmployeeTeamsAsync(team.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(team);
        _authorizationGateway
            .Setup(gateway => gateway.CanWriteAsync(User, It.IsAny<TeamResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new PatchTeamEmployees(
            _teamRepository.Object,
            _employeeRepository.Object,
            _authorizationGateway.Object,
            NullLogger<PatchTeamEmployees>.Instance);

        var result = await useCase.ExecuteAsync(User, team.Id, new PatchTeamEmployeesCommand([]));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
    }
}
