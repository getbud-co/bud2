using Bud.Api.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bud.Api.Features.Notifications;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.TenantSelected)]
[Route("api/notifications")]
[Produces("application/json")]
public sealed class NotificationsController(
    ListNotifications listNotifications,
    UpdateNotification updateNotification,
    UpdateAllNotifications updateAllNotifications,
    IValidator<PatchNotificationRequest> patchValidator) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> GetAll(
        [FromQuery] bool? isRead,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var paginationValidation = ValidatePagination(page, pageSize);
        if (paginationValidation is not null)
        {
            return paginationValidation;
        }

        var result = await listNotifications.ExecuteAsync(isRead, page, pageSize, cancellationToken);
        return FromResultOk(result, paged => paged.MapPaged(n => n.ToResponse()));
    }

    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponse>> Update(
        Guid id,
        PatchNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await patchValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var result = await updateNotification.ExecuteAsync(id, cancellationToken);
        return FromResultOk(result, notification => notification.ToResponse());
    }

    [HttpPatch]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAll(
        PatchNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await patchValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblemFrom(validationResult);
        }

        var result = await updateAllNotifications.ExecuteAsync(cancellationToken);
        return FromResult(result, NoContent);
    }
}
