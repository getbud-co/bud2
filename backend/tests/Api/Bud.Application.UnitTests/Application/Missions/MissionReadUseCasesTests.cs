using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Missions;

public sealed class MissionReadUseCasesTests
{
    private readonly Mock<IMissionRepository> _repo = new();
    private readonly Mock<IMissionProgressReadStore> _progressService = new();

    private GetMissionById CreateGetMissionById()
        => new(_repo.Object);

    private ListMissionProgress CreateListMissionProgress()
        => new(_progressService.Object);

    private ListMissions CreateListMissions()
        => new(_repo.Object);

    private ListMissionIndicators CreateListMissionMetrics()
        => new(_repo.Object);

    private ListMissionChildren CreateListMissionChildren()
        => new(_repo.Object);

    [Fact]
    public async Task GetByIdAsync_WithExistingMission_ReturnsSuccess()
    {
        var missionId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdReadOnlyAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mission { Id = missionId, Name = "M", OrganizationId = Guid.NewGuid() });

        var useCase = CreateGetMissionById();

        var result = await useCase.ExecuteAsync(missionId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(missionId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdReadOnlyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var useCase = CreateGetMissionById();

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetProgressAsync_DelegatesToProgressService()
    {
        var ids = new List<Guid> { Guid.NewGuid() };
        _progressService
            .Setup(s => s.GetProgressAsync(ids, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<MissionProgressSnapshot>>.Success([]));

        var useCase = CreateListMissionProgress();

        var result = await useCase.ExecuteAsync(ids);

        result.IsSuccess.Should().BeTrue();
        _progressService.Verify(s => s.GetProgressAsync(ids, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListMissionsAsync_DelegatesToRepository()
    {
        _repo.Setup(r => r.GetAllAsync(
                MissionFilter.All, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Mission> { Items = [], Total = 0, Page = 1, PageSize = 10 });

        var useCase = CreateListMissions();

        var result = await useCase.ExecuteAsync(MissionFilter.All, null, null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.GetAllAsync(MissionFilter.All, null, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMetricsAsync_WhenMissionNotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = CreateListMissionMetrics();

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), 1, 10);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
