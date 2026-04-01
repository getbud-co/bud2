using Bud.Application.Common;
using Bud.Application.Ports;
using FluentAssertions;
using System.Security.Claims;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Notifications;

public sealed class NotificationWriteUseCasesTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();
    private readonly Mock<IApplicationAuthorizationGateway> _authorizationGateway = new();
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity());

    private PatchNotification CreateMarkAsReadUseCase()
        => new(_repo.Object, _tenantProvider.Object, _authorizationGateway.Object);

    private PatchNotifications CreateMarkAllAsReadUseCase()
        => new(_repo.Object, _tenantProvider.Object, _authorizationGateway.Object);

    #region MarkAsReadAsync

    [Fact]
    public async Task MarkAsReadAsync_WithoutEmployee_ReturnsForbidden()
    {
        _tenantProvider.SetupGet(x => x.EmployeeId).Returns((Guid?)null);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<NotificationResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateMarkAsReadUseCase().ExecuteAsync(User, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Funcionário não identificado.");
    }

    [Fact]
    public async Task MarkAsReadAsync_WithNonExistentNotification_ReturnsNotFound()
    {
        _tenantProvider.SetupGet(x => x.EmployeeId).Returns(Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<NotificationResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateMarkAsReadUseCase().ExecuteAsync(User, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Notificação não encontrada.");
    }

    [Fact]
    public async Task MarkAsReadAsync_WithDifferentRecipient_ReturnsForbidden()
    {
        _tenantProvider.SetupGet(x => x.EmployeeId).Returns(Guid.NewGuid());
        var notificationId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = notificationId,
            RecipientEmployeeId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Title = "Test",
            Message = "Test",
            Type = NotificationType.MissionCreated,
            CreatedAtUtc = DateTime.UtcNow
        };
        _repo.Setup(r => r.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<NotificationResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateMarkAsReadUseCase().ExecuteAsync(User, notificationId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Você não tem permissão para marcar esta notificação como lida.");
    }

    [Fact]
    public async Task MarkAsReadAsync_AlreadyRead_ReturnsSuccessWithoutSaving()
    {
        _tenantProvider.SetupGet(x => x.EmployeeId).Returns(Guid.NewGuid());
        var notificationId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = notificationId,
            RecipientEmployeeId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Title = "Test",
            Message = "Test",
            Type = NotificationType.MissionCreated,
            IsRead = true,
            ReadAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };
        _repo.Setup(r => r.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<NotificationResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateMarkAsReadUseCase().ExecuteAsync(User, notificationId);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_WithValidNotification_MarksAndSaves()
    {
        var employeeId = Guid.NewGuid();
        _tenantProvider.SetupGet(x => x.EmployeeId).Returns(employeeId);
        var notificationId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = notificationId,
            RecipientEmployeeId = employeeId,
            OrganizationId = Guid.NewGuid(),
            Title = "Test",
            Message = "Test",
            Type = NotificationType.MissionCreated,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };
        _repo.Setup(r => r.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _authorizationGateway
            .Setup(g => g.CanWriteAsync(User, It.IsAny<NotificationResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateMarkAsReadUseCase().ExecuteAsync(User, notificationId);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        notification.ReadAtUtc.Should().NotBeNull();
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region MarkAllAsReadAsync

    [Fact]
    public async Task MarkAllAsReadAsync_WithoutEmployee_ReturnsForbidden()
    {
        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(x => x.EmployeeId).Returns((Guid?)null);
        _authorizationGateway
            .Setup(g => g.CanReadAsync(User, It.IsAny<NotificationInboxResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var useCase = new PatchNotifications(_repo.Object, tenantProvider.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(User);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_DelegatesToRepository()
    {
        var employeeId = Guid.NewGuid();
        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(x => x.EmployeeId).Returns(employeeId);
        _authorizationGateway
            .Setup(g => g.CanReadAsync(User, It.IsAny<NotificationInboxResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var useCase = new PatchNotifications(_repo.Object, tenantProvider.Object, _authorizationGateway.Object);

        var result = await useCase.ExecuteAsync(User);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.MarkAllAsReadAsync(employeeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
