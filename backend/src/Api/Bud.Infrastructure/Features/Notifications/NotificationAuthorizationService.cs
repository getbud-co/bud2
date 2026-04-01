using Bud.Application.Common;
using Bud.Application.Features.Notifications;
using Bud.Application.Ports;

namespace Bud.Infrastructure.Features.Notifications;

public sealed class NotificationAuthorizationService(
    INotificationRepository notificationRepository,
    ITenantProvider tenantProvider)
    : IReadAuthorizationRule<NotificationInboxResource>,
      IWriteAuthorizationRule<NotificationResource>
{
    private const string EmployeeNotIdentifiedMessage = "Colaborador não identificado.";
    private const string NotificationNotFoundMessage = "Notificação não encontrada.";
    private const string NotificationPatchForbiddenMessage = "Você não tem permissão para marcar esta notificação como lida.";

    public Task<Result> EvaluateAsync(NotificationInboxResource resource, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            tenantProvider.EmployeeId.HasValue
                ? Result.Success()
                : Result.Forbidden(EmployeeNotIdentifiedMessage));
    }

    async Task<Result> IWriteAuthorizationRule<NotificationResource>.EvaluateAsync(NotificationResource resource, CancellationToken cancellationToken)
    {
        if (!tenantProvider.EmployeeId.HasValue)
        {
            return Result.Forbidden(EmployeeNotIdentifiedMessage);
        }

        var notification = await notificationRepository.GetByIdAsync(resource.NotificationId, cancellationToken);
        if (notification is null)
        {
            return Result.NotFound(NotificationNotFoundMessage);
        }

        return notification.RecipientEmployeeId == tenantProvider.EmployeeId.Value
            ? Result.Success()
            : Result.Forbidden(NotificationPatchForbiddenMessage);
    }
}
