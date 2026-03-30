using Bud.Application.Common;
using Bud.Application.Features.Notifications;
using Bud.Application.Ports;
using Bud.Infrastructure.Features.Notifications;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public sealed class NotificationAuthorizationServiceTests
{
    [Fact]
    public async Task EvaluateWriteAsync_WhenEmployeeMissing_ReturnsForbidden()
    {
        var repository = new Mock<INotificationRepository>(MockBehavior.Strict);
        var tenantProvider = new TestTenantProvider { EmployeeId = null };
        var service = new NotificationAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<NotificationResource>)service)
            .EvaluateAsync(new NotificationResource(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Colaborador não identificado.");
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenRecipientMatches_ReturnsSuccess()
    {
        var employeeId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var repository = new Mock<INotificationRepository>();
        repository
            .Setup(r => r.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Notification { Id = notificationId, RecipientEmployeeId = employeeId });

        var tenantProvider = new TestTenantProvider { EmployeeId = employeeId };
        var service = new NotificationAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<NotificationResource>)service)
            .EvaluateAsync(new NotificationResource(notificationId));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenRecipientDiffers_ReturnsForbidden()
    {
        var notificationId = Guid.NewGuid();
        var repository = new Mock<INotificationRepository>();
        repository
            .Setup(r => r.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Notification { Id = notificationId, RecipientEmployeeId = Guid.NewGuid() });

        var tenantProvider = new TestTenantProvider { EmployeeId = Guid.NewGuid() };
        var service = new NotificationAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<NotificationResource>)service)
            .EvaluateAsync(new NotificationResource(notificationId));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Você não tem permissão para marcar esta notificação como lida.");
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenNotificationMissing_ReturnsNotFound()
    {
        var repository = new Mock<INotificationRepository>();
        repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var tenantProvider = new TestTenantProvider { EmployeeId = Guid.NewGuid() };
        var service = new NotificationAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<NotificationResource>)service)
            .EvaluateAsync(new NotificationResource(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Notificação não encontrada.");
    }
}
