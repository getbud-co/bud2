using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Missions;

public sealed class MissionWriteUseCasesTests
{
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity());
    private readonly Mock<IMissionRepository> _repo = new();
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();
    private readonly Mock<IApplicationAuthorizationGateway> _authorizationGateway = new();

    private CreateMission CreatePlanningUseCase()
        => new(_repo.Object, _employeeRepo.Object, _tenantProvider.Object, NullLogger<CreateMission>.Instance, _authorizationGateway.Object, null);

    private PatchMission CreateReplanningUseCase()
        => new(_repo.Object, _employeeRepo.Object, _tenantProvider.Object, NullLogger<PatchMission>.Instance, _authorizationGateway.Object, null);

    private DeleteMission CreateRemoveUseCase()
        => new(_repo.Object, _tenantProvider.Object, NullLogger<DeleteMission>.Instance, _authorizationGateway.Object, null);

    [Fact]
    public async Task CreateAsync_WhenTenantNotSelected_ReturnsForbidden()
    {
        _tenantProvider.SetupGet(t => t.TenantId).Returns((Guid?)null);

        var useCase = CreatePlanningUseCase();
        var command = new CreateMissionCommand("Missão", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned, null, null);

        var result = await useCase.ExecuteAsync(User, command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        _repo.Verify(r => r.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSuccess_RegistersMissionCreatedDomainEvent()
    {
        var orgId = Guid.NewGuid();
        _tenantProvider.SetupGet(t => t.TenantId).Returns(orgId);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<CreateMissionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = CreatePlanningUseCase();
        var command = new CreateMissionCommand("Missão", null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), MissionStatus.Planned, null, null);

        var result = await useCase.ExecuteAsync(User, command);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        var createdEvent = result.Value!.DomainEvents.Should().ContainSingle().Subject;
        var created = createdEvent.Should().BeOfType<MissionCreatedDomainEvent>().Subject;
        created.MissionId.Should().Be(result.Value.Id);
        created.OrganizationId.Should().Be(orgId);
    }

    [Fact]
    public async Task CreateAsync_WhenChildStartDateBeforeParent_ReturnsValidation()
    {
        var orgId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var parentMission = new Mission
        {
            Id = parentId,
            Name = "Missão pai",
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };

        _tenantProvider.SetupGet(t => t.TenantId).Returns(orgId);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<CreateMissionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repo.Setup(r => r.GetByIdReadOnlyAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentMission);

        var useCase = CreatePlanningUseCase();
        var command = new CreateMissionCommand(
            "Meta filha",
            null,
            null,
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), // Before parent's start
            new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            MissionStatus.Planned,
            parentId,
            null);

        var result = await useCase.ExecuteAsync(User, command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Contain("data de início");
        _repo.Verify(r => r.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenChildStartDateEqualsParent_Succeeds()
    {
        var orgId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var parentStartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var parentMission = new Mission
        {
            Id = parentId,
            Name = "Missão pai",
            StartDate = parentStartDate,
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };

        _tenantProvider.SetupGet(t => t.TenantId).Returns(orgId);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<CreateMissionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repo.Setup(r => r.GetByIdReadOnlyAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentMission);

        var useCase = CreatePlanningUseCase();
        var command = new CreateMissionCommand(
            "Meta filha",
            null,
            null,
            parentStartDate, // Same as parent — should be allowed
            new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            MissionStatus.Planned,
            parentId,
            null);

        var result = await useCase.ExecuteAsync(User, command);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenChildEndDateAfterParent_ReturnsValidation()
    {
        var orgId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var parentMission = new Mission
        {
            Id = parentId,
            Name = "Missão pai",
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };

        _tenantProvider.SetupGet(t => t.TenantId).Returns(orgId);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<CreateMissionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repo.Setup(r => r.GetByIdReadOnlyAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentMission);

        var useCase = CreatePlanningUseCase();
        var command = new CreateMissionCommand(
            "Meta filha",
            null,
            null,
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            MissionStatus.Planned,
            parentId,
            null);

        var result = await useCase.ExecuteAsync(User, command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Contain("data de término");
        _repo.Verify(r => r.AddAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PatchAsync_WhenChildStartDateBeforeParent_ReturnsValidation()
    {
        var orgId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var parentMission = new Mission
        {
            Id = parentId,
            Name = "Missão pai",
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };
        var childMission = new Mission
        {
            Id = missionId,
            ParentId = parentId,
            Name = "Meta filha",
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };

        _repo.Setup(r => r.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childMission);
        _repo.Setup(r => r.GetByIdReadOnlyAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentMission);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<MissionResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = CreateReplanningUseCase();
        var command = new PatchMissionCommand(
            default,
            default,
            default,
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), // Before parent's start
            default,
            default,
            default);

        var result = await useCase.ExecuteAsync(User, missionId, command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Contain("data de início");
    }

    [Fact]
    public async Task PatchAsync_WhenChildEndDateAfterParent_ReturnsValidation()
    {
        var orgId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var parentMission = new Mission
        {
            Id = parentId,
            Name = "Missão pai",
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };
        var childMission = new Mission
        {
            Id = missionId,
            ParentId = parentId,
            Name = "Meta filha",
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };

        _repo.Setup(r => r.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(childMission);
        _repo.Setup(r => r.GetByIdReadOnlyAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentMission);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<MissionResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = CreateReplanningUseCase();
        var command = new PatchMissionCommand(
            default,
            default,
            default,
            default,
            new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            default,
            default);

        var result = await useCase.ExecuteAsync(User, missionId, command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Contain("data de término");
    }

    [Fact]
    public async Task UpdateAsync_WhenMissionNotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var useCase = CreateReplanningUseCase();
        var command = new PatchMissionCommand(
            "Missão",
            default,
            default,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            MissionStatus.Planned,
            default);

        var result = await useCase.ExecuteAsync(User, Guid.NewGuid(), command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenSuccess_RegistersMissionUpdatedDomainEvent()
    {
        var orgId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var mission = new Mission
        {
            Id = missionId,
            Name = "Missão",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };

        _repo.Setup(r => r.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<MissionResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = CreateReplanningUseCase();
        var command = new PatchMissionCommand(
            "Missão Atualizada",
            default,
            default,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            MissionStatus.Active,
            default);

        var result = await useCase.ExecuteAsync(User, missionId, command);

        result.IsSuccess.Should().BeTrue();
        var updatedEvent = mission.DomainEvents.Should().ContainSingle().Subject;
        var updated = updatedEvent.Should().BeOfType<MissionUpdatedDomainEvent>().Subject;
        updated.MissionId.Should().Be(missionId);
        updated.OrganizationId.Should().Be(orgId);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var useCase = CreateRemoveUseCase();

        var result = await useCase.ExecuteAsync(User, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        _repo.Verify(r => r.RemoveAsync(It.IsAny<Mission>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccess_RegistersMissionDeletedDomainEvent()
    {
        var orgId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var mission = new Mission
        {
            Id = missionId,
            Name = "Missão",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = MissionStatus.Planned,
            OrganizationId = orgId
        };

        _repo.Setup(r => r.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<MissionResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = CreateRemoveUseCase();

        var result = await useCase.ExecuteAsync(User, missionId);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.RemoveAsync(mission, It.IsAny<CancellationToken>()), Times.Once);
        var deletedEvent = mission.DomainEvents.Should().ContainSingle().Subject;
        var deleted = deletedEvent.Should().BeOfType<MissionDeletedDomainEvent>().Subject;
        deleted.MissionId.Should().Be(missionId);
        deleted.OrganizationId.Should().Be(orgId);
    }
}
