using Bud.Api.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Api.UnitTests.Notifications;

public sealed class UpdateAllNotificationsTests
{
    [Fact]
    public async Task ExecuteAsync_WhenEmployeeContextMissing_ShouldReturnForbidden()
    {
        var repository = new Mock<INotificationRepository>();
        var tenantProvider = new TestTenantProvider { EmployeeId = null };
        var unitOfWork = new Mock<IUnitOfWork>();
        var sut = new UpdateAllNotifications(repository.Object, tenantProvider, NullLogger<UpdateAllNotifications>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnreadNotificationsExist_ShouldMarkAndCommit()
    {
        var repository = new Mock<INotificationRepository>();
        var tenantProvider = new TestTenantProvider { EmployeeId = Guid.NewGuid() };
        var unitOfWork = new Mock<IUnitOfWork>();
        var notifications = new List<Notification>
        {
            Notification.Create(Guid.NewGuid(), tenantProvider.EmployeeId.Value, Guid.NewGuid(), "A", "B", "system.info", DateTime.UtcNow),
            Notification.Create(Guid.NewGuid(), tenantProvider.EmployeeId.Value, Guid.NewGuid(), "C", "D", "system.info", DateTime.UtcNow)
        };

        repository.Setup(x => x.GetUnreadByRecipientAsync(tenantProvider.EmployeeId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var sut = new UpdateAllNotifications(repository.Object, tenantProvider, NullLogger<UpdateAllNotifications>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        notifications.Should().OnlyContain(x => x.IsRead);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
