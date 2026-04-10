using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Notifications;

public sealed class NotificationReadUseCasesTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();

    private ListNotifications CreateListUseCase()
        => new(_repo.Object, _tenantProvider.Object);

    #region GetMyNotificationsAsync

    [Fact]
    public async Task GetMyNotificationsAsync_ReturnsPagedNotifications()
    {
        var employeeId = Guid.NewGuid();
        _tenantProvider.SetupGet(x => x.EmployeeId).Returns(employeeId);

        var pagedResult = new PagedResult<Notification>
        {
            Items = [new Notification { Id = Guid.NewGuid(), Title = "Test", Message = "Msg", Type = NotificationType.MissionCreated }],
            Total = 1,
            Page = 1,
            PageSize = 10
        };
        _repo.Setup(r => r.GetByRecipientAsync(employeeId, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await CreateListUseCase().ExecuteAsync(null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Total.Should().Be(1);
        _repo.Verify(r => r.GetByRecipientAsync(employeeId, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
