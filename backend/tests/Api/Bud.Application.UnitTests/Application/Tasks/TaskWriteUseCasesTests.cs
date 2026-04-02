using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Tasks;

public sealed class TaskWriteUseCasesTests
{
    private static Mission MakeMission() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Meta de Teste",
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddDays(30),
        Status = MissionStatus.Active,
        OrganizationId = Guid.NewGuid()
    };

    private static MissionTask MakeTask(Guid? orgId = null, Guid? missionId = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Tarefa de Teste",
        State = TaskState.ToDo,
        OrganizationId = orgId ?? Guid.NewGuid(),
        MissionId = missionId ?? Guid.NewGuid()
    };

    // ── CreateTask ──────────────────────────────────────────────

    [Fact]
    public async Task CreateTask_WhenMissionNotFound_ReturnsNotFound()
    {
        var missionId = Guid.NewGuid();
        var repo = new Mock<ITaskRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetMissionByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var useCase = new CreateTask(repo.Object, NullLogger<CreateTask>.Instance, null);

        var result = await useCase.ExecuteAsync(new CreateTaskCommand(missionId, "Tarefa", null, TaskState.ToDo, null));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateTask_WhenAuthorized_CreatesTaskViaRepository()
    {
        var mission = MakeMission();
        var repo = new Mock<ITaskRepository>();

        repo.Setup(r => r.GetMissionByIdAsync(mission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);
        repo.Setup(r => r.AddAsync(It.IsAny<MissionTask>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new CreateTask(repo.Object, NullLogger<CreateTask>.Instance, null);

        var result = await useCase.ExecuteAsync(new CreateTaskCommand(
            mission.Id,
            "Implementar feature",
            "Detalhes da feature",
            TaskState.Doing,
            null));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Implementar feature");
        result.Value.MissionId.Should().Be(mission.Id);
        result.Value.OrganizationId.Should().Be(mission.OrganizationId);
        result.Value.State.Should().Be(TaskState.Doing);
        repo.Verify(r => r.AddAsync(It.IsAny<MissionTask>(), It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTask_WithDueDate_ResponseContainsDueDate()
    {
        var mission = MakeMission();
        var dueDate = new DateTime(2026, 12, 31);
        var repo = new Mock<ITaskRepository>();

        repo.Setup(r => r.GetMissionByIdAsync(mission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mission);
        repo.Setup(r => r.AddAsync(It.IsAny<MissionTask>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new CreateTask(repo.Object, NullLogger<CreateTask>.Instance, null);

        var result = await useCase.ExecuteAsync(new CreateTaskCommand(mission.Id, "Tarefa com prazo", null, TaskState.ToDo, dueDate));

        result.IsSuccess.Should().BeTrue();
        result.Value!.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public async Task GetTaskById_WhenTaskExists_ReturnsSuccess()
    {
        var task = MakeTask();
        var repo = new Mock<ITaskRepository>();

        repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var useCase = new GetTaskById(repo.Object);

        var result = await useCase.ExecuteAsync(task.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(task);
    }

    [Fact]
    public async Task GetTaskById_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        var repo = new Mock<ITaskRepository>();

        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MissionTask?)null);

        var useCase = new GetTaskById(repo.Object);

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Tarefa não encontrada.");
    }

    // ── PatchTask ───────────────────────────────────────────────

    [Fact]
    public async Task PatchTask_WhenNotFound_ReturnsNotFound()
    {
        var repo = new Mock<ITaskRepository>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MissionTask?)null);

        var useCase = new PatchTask(repo.Object, NullLogger<PatchTask>.Instance, null);

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), new PatchTaskCommand(default, default, default, default));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task PatchTask_WhenAuthorized_UpdatesAndReturnsSuccess()
    {
        var task = MakeTask();
        var repo = new Mock<ITaskRepository>();

        repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new PatchTask(repo.Object, NullLogger<PatchTask>.Instance, null);

        var result = await useCase.ExecuteAsync(task.Id, new PatchTaskCommand(default, default, TaskState.Done, default));

        result.IsSuccess.Should().BeTrue();
        result.Value!.State.Should().Be(TaskState.Done);
    }

    [Fact]
    public async Task PatchTask_WithDueDate_ResponseContainsDueDate()
    {
        var task = MakeTask();
        var dueDate = new DateTime(2026, 9, 30);
        var repo = new Mock<ITaskRepository>();

        repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new PatchTask(repo.Object, NullLogger<PatchTask>.Instance, null);

        var result = await useCase.ExecuteAsync(task.Id, new PatchTaskCommand(default, default, default, (DateTime?)dueDate));

        result.IsSuccess.Should().BeTrue();
        result.Value!.DueDate.Should().Be(dueDate);
    }

    // ── DeleteTask ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteTask_WhenNotFound_ReturnsNotFound()
    {
        var repo = new Mock<ITaskRepository>(MockBehavior.Strict);

        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MissionTask?)null);

        var useCase = new DeleteTask(repo.Object, NullLogger<DeleteTask>.Instance, null);

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteTask_WhenAuthorized_RemovesAndReturnsSuccess()
    {
        var task = MakeTask();
        var repo = new Mock<ITaskRepository>();

        repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        repo.Setup(r => r.RemoveAsync(task, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new DeleteTask(repo.Object, NullLogger<DeleteTask>.Instance, null);

        var result = await useCase.ExecuteAsync(task.Id);

        result.IsSuccess.Should().BeTrue();
        repo.Verify(r => r.RemoveAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
