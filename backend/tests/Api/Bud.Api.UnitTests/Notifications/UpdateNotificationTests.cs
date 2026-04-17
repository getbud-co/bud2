using Bud.Api.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Api.UnitTests.Notifications;

public sealed class UpdateNotificationTests
{
    [Fact]
    public async Task ExecuteAsync_WhenEmployeeContextMissing_ShouldReturnForbidden()
    {
        var repository = new Mock<INotificationRepository>();
        var tenantProvider = new TestTenantProvider { EmployeeId = null };
        var unitOfWork = new Mock<IUnitOfWork>();
        var sut = new UpdateNotification(repository.Object, tenantProvider, NullLogger<UpdateNotification>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotificationBelongsToAnotherRecipient_ShouldReturnNotFound()
    {
        var repository = new Mock<INotificationRepository>();
        var tenantProvider = new TestTenantProvider { EmployeeId = Guid.NewGuid() };
        var unitOfWork = new Mock<IUnitOfWork>();
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Teste",
            "Mensagem",
            "system.info",
            DateTime.UtcNow);

        repository.Setup(x => x.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var sut = new UpdateNotification(repository.Object, tenantProvider, NullLogger<UpdateNotification>.Instance, unitOfWork.Object);

        var result = await sut.ExecuteAsync(notification.Id);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
