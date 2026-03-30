using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Missions;

public sealed class MissionObjectiveReadUseCasesTests
{
    private readonly Mock<IMissionRepository> _repository = new();
    private readonly Mock<IMissionProgressReadStore> _progressService = new();
    private readonly Mock<IApplicationAuthorizationGateway> _authorizationGateway = new();

    [Fact]
    public async Task ViewMissionObjectiveDetails_WhenFound_ReturnsObjective()
    {
        var missionId = Guid.NewGuid();
        var objective = new Mission { Id = missionId, OrganizationId = Guid.NewGuid(), Name = "Obj" };

        _repository
            .Setup(repository => repository.GetByIdReadOnlyAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(objective);
        _authorizationGateway
            .Setup(g => g.CanReadAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<MissionResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new GetMissionById(_repository.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(new ClaimsIdentity()), missionId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(missionId);
    }

    [Fact]
    public async Task ViewMissionObjectiveDetails_WhenNotFound_ReturnsNotFound()
    {
        _repository
            .Setup(repository => repository.GetByIdReadOnlyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var useCase = new GetMissionById(_repository.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(new ClaimsIdentity()), Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ListMissionObjectives_ReturnsPagedResult()
    {
        var parentId = Guid.NewGuid();

        var pagedResult = new PagedResult<Mission>
        {
            Items = [],
            Total = 0,
            Page = 1,
            PageSize = 10
        };

        _repository
            .Setup(repository => repository.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repository
            .Setup(repository => repository.GetChildrenAsync(parentId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _authorizationGateway
            .Setup(g => g.CanReadAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<MissionResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new ListMissionChildren(_repository.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(new ClaimsPrincipal(new ClaimsIdentity()), parentId, 1, 10);

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(repository => repository.GetChildrenAsync(parentId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CalculateMissionObjectiveProgress_DelegatesToProgressService()
    {
        var missionIds = new List<Guid> { Guid.NewGuid() };

        _progressService
            .Setup(service => service.GetProgressAsync(missionIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<MissionProgressSnapshot>>.Success([]));

        var useCase = new ListMissionProgress(_progressService.Object);

        var result = await useCase.ExecuteAsync(missionIds);

        result.IsSuccess.Should().BeTrue();
        _progressService.Verify(service => service.GetProgressAsync(missionIds, It.IsAny<CancellationToken>()), Times.Once);
    }
}
