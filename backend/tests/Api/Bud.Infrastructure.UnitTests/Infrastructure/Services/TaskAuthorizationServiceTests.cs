using Bud.Application.Common;
using Bud.Application.Features.Tasks;
using Bud.Domain.Missions;
using Bud.Infrastructure.Features.Tasks;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public sealed class TaskAuthorizationServiceTests
{
    [Fact]
    public async Task EvaluateWriteAsync_WhenTaskMissing_ReturnsNotFound()
    {
        var repository = new Mock<ITaskRepository>();
        repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MissionTask?)null);

        var tenantProvider = new TestTenantProvider { EmployeeId = Guid.NewGuid() };
        var service = new TaskAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<TaskResource>)service)
            .EvaluateAsync(new TaskResource(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Tarefa não encontrada.");
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenTenantMatches_ReturnsSuccess()
    {
        var organizationId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var repository = new Mock<ITaskRepository>();
        repository
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MissionTask { Id = taskId, OrganizationId = organizationId });

        var tenantProvider = new TestTenantProvider
        {
            TenantId = organizationId,
            EmployeeId = Guid.NewGuid()
        };
        var service = new TaskAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<TaskResource>)service)
            .EvaluateAsync(new TaskResource(taskId));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateCreateAsync_WhenMissionMissing_ReturnsNotFound()
    {
        var repository = new Mock<ITaskRepository>();
        repository
            .Setup(r => r.GetMissionByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mission?)null);

        var tenantProvider = new TestTenantProvider();
        var service = new TaskAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<CreateTaskContext>)service)
            .EvaluateAsync(new CreateTaskContext(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Meta não encontrada.");
    }
}
