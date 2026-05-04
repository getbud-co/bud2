namespace Bud.Domain.Tests.Notifications;

public sealed class NotificationDomainTests
{
    [Fact]
    public void Create_WithValidPayload_ShouldInitializeAsUnread()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Infraestrutura disponível",
            "O serviço de notificações foi iniciado.",
            "system.info",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "System");

        notification.IsRead.Should().BeFalse();
        notification.Category.Should().Be("system.info");
        notification.ReferenceType.Should().Be("System");
    }

    [Fact]
    public void MarkAsRead_ShouldUpdateReadStateOnce()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Infraestrutura disponível",
            "O serviço de notificações foi iniciado.",
            "system.info",
            DateTime.UtcNow);

        var readAt = DateTime.UtcNow;
        notification.MarkAsRead(readAt);
        notification.MarkAsRead(readAt.AddMinutes(1));

        notification.IsRead.Should().BeTrue();
        notification.ReadAtUtc.Should().Be(readAt);
    }
}
