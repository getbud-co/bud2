using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Notifications.UseCases;

public sealed class ListNotifications(
    INotificationRepository notificationRepository,
    IApplicationAuthorizationGateway authorizationGateway,
    ITenantProvider tenantProvider)
{
    public async Task<Result<PagedResult<NotificationResponse>>> ExecuteAsync(
        ClaimsPrincipal user,
        bool? isRead,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (!await authorizationGateway.CanReadAsync(user, NotificationInboxResource.Instance, cancellationToken))
        {
            return Result<PagedResult<NotificationResponse>>.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

        var pagedSummaries = await notificationRepository.GetByRecipientAsync(
            tenantProvider.EmployeeId!.Value,
            isRead,
            page,
            pageSize,
            cancellationToken);

        return Result<PagedResult<NotificationResponse>>.Success(pagedSummaries.MapPaged(n => n.ToResponse()));
    }
}
