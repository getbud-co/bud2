using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Notifications.UseCases;

public sealed class PatchNotifications(
    INotificationRepository notificationRepository,
    ITenantProvider tenantProvider,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result> ExecuteAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (!await authorizationGateway.CanReadAsync(user, NotificationInboxResource.Instance, cancellationToken))
        {
            return Result.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

        if (tenantProvider.EmployeeId is null)
        {
            return Result.Forbidden(UserErrorMessages.EmployeeNotIdentified);
        }

        await notificationRepository.MarkAllAsReadAsync(
            tenantProvider.EmployeeId.Value,
            cancellationToken);

        return Result.Success();
    }
}
