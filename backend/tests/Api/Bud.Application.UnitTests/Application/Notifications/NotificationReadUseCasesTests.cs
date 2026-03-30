using Bud.Application.Common;
using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentAssertions;
using System.Security.Claims;
using Moq;
using Xunit;

namespace Bud.Application.UnitTests.Application.Notifications;

public sealed class NotificationReadUseCasesTests
{
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<ITenantProvider> _tenantProvider = new();
    private readonly Mock<IApplicationAuthorizationGateway> _authorizationGateway = new();
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity());

    private ListNotifications CreateListUseCase()
        => new(_repo.Object, _authorizationGateway.Object, _tenantProvider.Object);

    #region GetMyNotificationsAsync

    [Fact]
    public async Task GetMyNotificationsAsync_WithoutEmployee_ReturnsForbidden()
    {
        _tenantProvider.SetupGet(x => x.EmployeeId).Returns((Guid?)null);
        _authorizationGateway
            .Setup(g => g.CanReadAsync(User, It.IsAny<NotificationInboxResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateListUseCase().ExecuteAsync(User, null, 1, 10);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Funcionário não identificado.");
        _repo.Verify(r => r.GetByRecipientAsync(It.IsAny<Guid>(), It.IsAny<bool?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMyNotificationsAsync_ReturnsPagedNotifications()
    {
        var employeeId = Guid.NewGuid();
        _tenantProvider.SetupGet(x => x.EmployeeId).Returns(employeeId);
        _authorizationGateway
            .Setup(g => g.CanReadAsync(User, It.IsAny<NotificationInboxResource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var pagedResult = new PagedResult<Notification>
        {
            Items = [new Notification { Id = Guid.NewGuid(), Title = "Test", Message = "Msg", Type = NotificationType.MissionCreated }],
            Total = 1,
            Page = 1,
            PageSize = 10
        };
        _repo.Setup(r => r.GetByRecipientAsync(employeeId, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await CreateListUseCase().ExecuteAsync(User, null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Total.Should().Be(1);
        _repo.Verify(r => r.GetByRecipientAsync(employeeId, null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
