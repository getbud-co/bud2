using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Missions;

public sealed class MissionObjectiveWriteUseCasesTests
{
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity());
    private readonly Mock<IMissionRepository> _repository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();
    private readonly Mock<IApplicationAuthorizationGateway> _authorizationGateway = new();

    [Fact]
    public async Task DefineMissionObjective_WhenTenantNotSelected_ReturnsForbidden()
    {
        _tenantProvider.SetupGet(t => t.TenantId).Returns((Guid?)null);

        var useCase = new CreateMission(_repository.Object, _employeeRepository.Object, _tenantProvider.Object, NullLogger<CreateMission>.Instance, _authorizationGateway.Object, null);

        var result = await useCase.ExecuteAsync(User, new CreateMissionCommand(
            "Objetivo",
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            MissionStatus.Planned,
            Guid.NewGuid(),
            null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        _repository.Verify(r => r.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DefineMissionObjective_WhenAuthorized_CreatesObjective()
    {
        var organizationId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        _tenantProvider.SetupGet(t => t.TenantId).Returns(organizationId);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<CreateMissionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var parentMission = new Mission
        {
            Id = parentId,
            Name = "Missão pai",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(60),
            Status = MissionStatus.Planned,
            OrganizationId = organizationId
        };

        _repository
            .Setup(r => r.GetByIdReadOnlyAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentMission);

        var useCase = new CreateMission(_repository.Object, _employeeRepository.Object, _tenantProvider.Object, NullLogger<CreateMission>.Instance, _authorizationGateway.Object, null);

        var result = await useCase.ExecuteAsync(User, new CreateMissionCommand(
            "Objetivo",
            null,
            "Clientes",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            MissionStatus.Planned,
            parentId,
            null));

        result.IsSuccess.Should().BeTrue();
        result.Value!.OrganizationId.Should().Be(organizationId);
        result.Value.ParentId.Should().Be(parentId);
        result.Value.Dimension.Should().Be("Clientes");
        _repository.Verify(r => r.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReviseMissionObjective_WhenObjectiveNotFound_ReturnsNotFound()
    {
        _repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var useCase = new PatchMission(_repository.Object, _employeeRepository.Object, _tenantProvider.Object, NullLogger<PatchMission>.Instance, _authorizationGateway.Object, null);

        var result = await useCase.ExecuteAsync(User, Guid.NewGuid(), new PatchMissionCommand("X", default, default, default, default, default, default));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ReviseMissionObjective_WhenAuthorized_UpdatesObjective()
    {
        var organizationId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var objective = new Mission
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ParentId = parentId,
            Name = "Obj",
            Status = MissionStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        _repository
            .Setup(r => r.GetByIdAsync(objective.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(objective);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<MissionResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new PatchMission(_repository.Object, _employeeRepository.Object, _tenantProvider.Object, NullLogger<PatchMission>.Instance, _authorizationGateway.Object, null);

        var result = await useCase.ExecuteAsync(User, objective.Id, new PatchMissionCommand(
            "Atualizado",
            "Nova descrição",
            default,
            default,
            default,
            default,
            default));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Atualizado");
    }

    [Fact]
    public async Task RemoveMissionObjective_WhenNotFound_ReturnsNotFound()
    {
        _repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var useCase = new DeleteMission(_repository.Object, _tenantProvider.Object, NullLogger<DeleteMission>.Instance, _authorizationGateway.Object, null);

        var result = await useCase.ExecuteAsync(User, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _repository.Verify(r => r.RemoveAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
