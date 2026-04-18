namespace Bud.Application.Tests.Notifications;

public sealed class CreateNotificationsTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPersistDistinctRecipients()
    {
        var repository = new Mock<INotificationRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var sut = new CreateNotifications(repository.Object, NullLogger<CreateNotifications>.Instance, unitOfWork.Object);
        List<Notification>? captured = null;

        repository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Notification>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Notification>, CancellationToken>((notifications, _) => captured = notifications.ToList())
            .Returns(Task.CompletedTask);

        var recipients = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var command = new CreateNotificationsCommand(
            [recipients[0], recipients[1], recipients[1], recipients[2]],
            Guid.NewGuid(),
            "Infra pronta",
            "Canal disponível para uso.",
            "system.info");

        var result = await sut.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Should().HaveCount(3);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
