using Bud.Application.EventHandlers;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.EventHandlers;

public sealed class DomainEventNotificationHandlersTests
{
    private static Mock<NotificationOrchestrator> CreateOrchestratorMock()
    {
        var repositoryMock = new Mock<INotificationRepository>();
        var recipientResolverMock = new Mock<INotificationRecipientResolver>();
        return new Mock<NotificationOrchestrator>(
            repositoryMock.Object,
            recipientResolverMock.Object);
    }

    [Fact]
    public async Task MissionCreatedHandler_ShouldNotifyMissionCreated()
    {
        var orchestratorMock = CreateOrchestratorMock();
        orchestratorMock
            .Setup(o => o.NotifyMissionCreatedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new MissionCreatedDomainEventNotifier(orchestratorMock.Object);
        var domainEvent = new MissionCreatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "Meta", Guid.NewGuid());

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        orchestratorMock.Verify(
            o => o.NotifyMissionCreatedAsync(
                domainEvent.MissionId,
                domainEvent.OrganizationId,
                domainEvent.MissionName,
                domainEvent.ActorEmployeeId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MissionUpdatedHandler_ShouldNotifyMissionUpdated()
    {
        var orchestratorMock = CreateOrchestratorMock();
        orchestratorMock
            .Setup(o => o.NotifyMissionUpdatedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new MissionUpdatedDomainEventNotifier(orchestratorMock.Object);
        var domainEvent = new MissionUpdatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "Meta", Guid.NewGuid());

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        orchestratorMock.Verify(
            o => o.NotifyMissionUpdatedAsync(
                domainEvent.MissionId,
                domainEvent.OrganizationId,
                domainEvent.MissionName,
                domainEvent.ActorEmployeeId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MissionDeletedHandler_ShouldNotifyMissionDeleted()
    {
        var orchestratorMock = CreateOrchestratorMock();
        orchestratorMock
            .Setup(o => o.NotifyMissionDeletedAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new MissionDeletedDomainEventNotifier(orchestratorMock.Object);
        var domainEvent = new MissionDeletedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "Meta", Guid.NewGuid());

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        orchestratorMock.Verify(
            o => o.NotifyMissionDeletedAsync(
                domainEvent.MissionId,
                domainEvent.OrganizationId,
                domainEvent.MissionName,
                domainEvent.ActorEmployeeId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckinCreatedHandler_ShouldNotifyCheckinCreated()
    {
        var orchestratorMock = CreateOrchestratorMock();
        orchestratorMock
            .Setup(o => o.NotifyCheckinCreatedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CheckinCreatedDomainEventNotifier(orchestratorMock.Object);
        var domainEvent = new CheckinCreatedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Vendas mensais");

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        orchestratorMock.Verify(
            o => o.NotifyCheckinCreatedAsync(
                domainEvent.CheckinId,
                domainEvent.IndicatorId,
                domainEvent.OrganizationId,
                domainEvent.EmployeeId,
                domainEvent.IndicatorName,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
