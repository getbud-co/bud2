using Bud.Application.EventHandlers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.EventHandlers;

public class NotificationOrchestratorTests
{
    private readonly Mock<INotificationRepository> _repoMock = new();
    private readonly Mock<INotificationRecipientResolver> _recipientResolverMock = new();
    private readonly NotificationOrchestrator _orchestrator;

    public NotificationOrchestratorTests()
    {
        _orchestrator = new NotificationOrchestrator(
            _repoMock.Object,
            _recipientResolverMock.Object);
    }

    [Fact]
    public async Task NotifyMissionCreatedAsync_WithActorAndRecipients_CreatesNotificationsWithActorName()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var recipients = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _recipientResolverMock
            .Setup(r => r.ResolveMissionRecipientsAsync(missionId, organizationId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipients);

        _recipientResolverMock
            .Setup(r => r.ResolveEmployeeNameAsync(actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Maria Silva");

        // Act
        await _orchestrator.NotifyMissionCreatedAsync(missionId, organizationId, "Aumentar vendas Q1", actorId);

        // Assert
        _repoMock.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<Notification>>(n =>
                    n.Count() == 2 &&
                    n.All(x => x.Type == NotificationType.MissionCreated &&
                               x.Title == "Nova meta criada" &&
                               x.Message == "Maria Silva criou a meta 'Aumentar vendas Q1'")),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifyMissionCreatedAsync_WithoutActor_CreatesNotificationsWithFallbackMessage()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var recipients = new List<Guid> { Guid.NewGuid() };

        _recipientResolverMock
            .Setup(r => r.ResolveMissionRecipientsAsync(missionId, organizationId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipients);

        // Act
        await _orchestrator.NotifyMissionCreatedAsync(missionId, organizationId, "Aumentar vendas Q1", null);

        // Assert
        _repoMock.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<Notification>>(n =>
                    n.All(x => x.Message == "Uma nova meta foi criada: 'Aumentar vendas Q1'")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyMissionCreatedAsync_WithEmptyRecipients_DoesNotCreateNotifications()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        _recipientResolverMock
            .Setup(r => r.ResolveMissionRecipientsAsync(missionId, organizationId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid>());

        // Act
        await _orchestrator.NotifyMissionCreatedAsync(missionId, organizationId, "Meta", null);

        // Assert
        _repoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifyMissionUpdatedAsync_WithActor_CreatesNotificationsWithActorName()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var recipients = new List<Guid> { Guid.NewGuid() };

        _recipientResolverMock
            .Setup(r => r.ResolveMissionRecipientsAsync(missionId, organizationId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipients);

        _recipientResolverMock
            .Setup(r => r.ResolveEmployeeNameAsync(actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("João Souza");

        // Act
        await _orchestrator.NotifyMissionUpdatedAsync(missionId, organizationId, "Aumentar vendas Q1", actorId);

        // Assert
        _repoMock.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<Notification>>(n =>
                    n.Count() == 1 &&
                    n.All(x => x.Type == NotificationType.MissionUpdated &&
                               x.Title == "Meta atualizada" &&
                               x.Message == "João Souza atualizou a meta 'Aumentar vendas Q1'")),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifyMissionDeletedAsync_WithActor_CreatesNotificationsWithActorName()
    {
        // Arrange
        var missionId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var recipients = new List<Guid> { Guid.NewGuid() };

        _recipientResolverMock
            .Setup(r => r.ResolveMissionRecipientsAsync(missionId, organizationId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipients);

        _recipientResolverMock
            .Setup(r => r.ResolveEmployeeNameAsync(actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Maria Silva");

        // Act
        await _orchestrator.NotifyMissionDeletedAsync(missionId, organizationId, "Aumentar vendas Q1", actorId);

        // Assert
        _repoMock.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<Notification>>(n =>
                    n.Count() == 1 &&
                    n.All(x => x.Type == NotificationType.MissionDeleted &&
                               x.Title == "Meta removida" &&
                               x.Message == "Maria Silva removeu a meta 'Aumentar vendas Q1'")),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifyCheckinCreatedAsync_WithActorAndRecipients_CreatesNotificationsWithIndicatorName()
    {
        // Arrange
        var checkinId = Guid.NewGuid();
        var indicatorId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var recipients = new List<Guid> { Guid.NewGuid() };

        _recipientResolverMock
            .Setup(r => r.ResolveMissionIdFromIndicatorAsync(indicatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(missionId);

        _recipientResolverMock
            .Setup(r => r.ResolveMissionRecipientsAsync(missionId, organizationId, actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipients);

        _recipientResolverMock
            .Setup(r => r.ResolveEmployeeNameAsync(actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("João Souza");

        // Act
        await _orchestrator.NotifyCheckinCreatedAsync(checkinId, indicatorId, organizationId, actorId, "Vendas mensais");

        // Assert
        _repoMock.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<Notification>>(n =>
                    n.Count() == 1 &&
                    n.All(x => x.Type == NotificationType.CheckinCreated &&
                               x.Title == "Novo check-in registrado" &&
                               x.Message == "João Souza registrou um check-in no indicador 'Vendas mensais'" &&
                               x.RelatedEntityId == checkinId &&
                               x.RelatedEntityType == "Checkin")),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifyCheckinCreatedAsync_WhenMissionNotFound_DoesNotCreateNotifications()
    {
        // Arrange
        var checkinId = Guid.NewGuid();
        var indicatorId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        _recipientResolverMock
            .Setup(r => r.ResolveMissionIdFromIndicatorAsync(indicatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        // Act
        await _orchestrator.NotifyCheckinCreatedAsync(checkinId, indicatorId, organizationId, null, "Indicador");

        // Assert
        _recipientResolverMock.Verify(
            r => r.ResolveMissionRecipientsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NotifyCheckinCreatedAsync_WithEmptyRecipients_DoesNotCreateNotifications()
    {
        // Arrange
        var checkinId = Guid.NewGuid();
        var indicatorId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var missionId = Guid.NewGuid();

        _recipientResolverMock
            .Setup(r => r.ResolveMissionIdFromIndicatorAsync(indicatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(missionId);

        _recipientResolverMock
            .Setup(r => r.ResolveMissionRecipientsAsync(missionId, organizationId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid>());

        // Act
        await _orchestrator.NotifyCheckinCreatedAsync(checkinId, indicatorId, organizationId, null, "Indicador");

        // Assert
        _repoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
