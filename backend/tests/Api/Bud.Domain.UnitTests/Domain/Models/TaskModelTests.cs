using Bud.Shared.Contracts;
using FluentAssertions;
using Xunit;

namespace Bud.Domain.UnitTests.Domain.Models;

public sealed class TaskModelTests
{
    [Fact]
    public void MissionTask_Create_WithEmptyOrganizationId_ShouldThrow()
    {
        var act = () => MissionTask.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), "Tarefa", null, TaskState.ToDo);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void MissionTask_Create_WithEmptyMissionId_ShouldThrow()
    {
        var act = () => MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "Tarefa", null, TaskState.ToDo);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void MissionTask_Create_WithEmptyName_ShouldThrow()
    {
        var act = () => MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  ", null, TaskState.ToDo);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void MissionTask_Create_WithNameExceeding200Chars_ShouldThrow()
    {
        var longName = new string('A', 201);

        var act = () => MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), longName, null, TaskState.ToDo);

        act.Should().Throw<DomainInvariantException>();
    }

    [Fact]
    public void MissionTask_Create_WithValidData_ShouldSetProperties()
    {
        var id = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var missionId = Guid.NewGuid();

        var task = MissionTask.Create(id, orgId, missionId, "  Minha Tarefa  ", "Descrição da tarefa", TaskState.Doing);

        task.Id.Should().Be(id);
        task.OrganizationId.Should().Be(orgId);
        task.MissionId.Should().Be(missionId);
        task.Name.Should().Be("Minha Tarefa");
        task.Description.Should().Be("Descrição da tarefa");
        task.State.Should().Be(TaskState.Doing);
    }

    [Fact]
    public void MissionTask_Create_WithWhitespaceDescription_ShouldSetDescriptionToNull()
    {
        var task = MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Tarefa", "   ", TaskState.ToDo);

        task.Description.Should().BeNull();
    }

    [Fact]
    public void MissionTask_UpdateDetails_ShouldChangeStateAndDescription()
    {
        var task = MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Tarefa", null, TaskState.ToDo);

        task.UpdateDetails("Tarefa Atualizada", "Nova descrição", TaskState.Done, null);

        task.Name.Should().Be("Tarefa Atualizada");
        task.Description.Should().Be("Nova descrição");
        task.State.Should().Be(TaskState.Done);
    }

    [Fact]
    public void MissionTask_Create_WithDueDate_SetsDueDate()
    {
        var dueDate = new DateTime(2026, 12, 31);

        var task = MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Tarefa", null, TaskState.ToDo, dueDate);

        task.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public void MissionTask_Create_WithNullDueDate_DueDateIsNull()
    {
        var task = MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Tarefa", null, TaskState.ToDo);

        task.DueDate.Should().BeNull();
    }

    [Fact]
    public void MissionTask_UpdateDetails_WithDueDate_UpdatesDueDate()
    {
        var task = MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Tarefa", null, TaskState.ToDo);
        var dueDate = new DateTime(2026, 6, 15);

        task.UpdateDetails("Tarefa", null, TaskState.ToDo, dueDate);

        task.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public void MissionTask_UpdateDetails_ClearDueDate_SetsDueDateToNull()
    {
        var task = MissionTask.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Tarefa", null, TaskState.ToDo, new DateTime(2026, 6, 15));

        task.UpdateDetails("Tarefa", null, TaskState.ToDo, null);

        task.DueDate.Should().BeNull();
    }
}
