namespace Bud.Application.Tests.Notifications;

public sealed class ListNotificationsTests
{
    [Fact]
    public async Task ExecuteAsync_WhenEmployeeContextMissing_ShouldReturnForbidden()
    {
        var repository = new Mock<INotificationRepository>();
        var tenantProvider = new TestTenantProvider { EmployeeId = null };
        var sut = new ListNotifications(repository.Object, tenantProvider);

        var result = await sut.ExecuteAsync(null, 1, 10);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
    }
}
