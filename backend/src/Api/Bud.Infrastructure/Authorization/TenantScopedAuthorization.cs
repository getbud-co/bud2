using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Infrastructure.Authorization;

public static class TenantScopedAuthorization
{
    public static async Task<Result> AuthorizeReadAsync<T>(
        ITenantProvider tenantProvider,
        Func<CancellationToken, Task<T?>> loadAsync,
        Func<T, Guid> organizationIdSelector,
        string notFoundMessage,
        string forbiddenMessage,
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return Result.Success();
        }

        var resource = await loadAsync(cancellationToken);
        if (resource is null)
        {
            return Result.NotFound(notFoundMessage);
        }

        return tenantProvider.TenantId.HasValue && organizationIdSelector(resource) == tenantProvider.TenantId.Value
            ? Result.Success()
            : Result.Forbidden(forbiddenMessage);
    }

    public static async Task<Result> AuthorizeWriteAsync<T>(
        ITenantProvider tenantProvider,
        Func<CancellationToken, Task<T?>> loadAsync,
        Func<T, Guid> organizationIdSelector,
        string notFoundMessage,
        string employeeNotIdentifiedMessage,
        string forbiddenMessage,
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return Result.Success();
        }

        if (tenantProvider.EmployeeId is null)
        {
            return Result.Forbidden(employeeNotIdentifiedMessage);
        }

        var resource = await loadAsync(cancellationToken);
        if (resource is null)
        {
            return Result.NotFound(notFoundMessage);
        }

        return tenantProvider.TenantId.HasValue && organizationIdSelector(resource) == tenantProvider.TenantId.Value
            ? Result.Success()
            : Result.Forbidden(forbiddenMessage);
    }

    public static Task<Result> AuthorizeWriteAsync(
        ITenantProvider tenantProvider,
        Guid organizationId,
        string employeeNotIdentifiedMessage,
        string forbiddenMessage)
    {
        if (tenantProvider.IsGlobalAdmin)
        {
            return Task.FromResult(Result.Success());
        }

        if (tenantProvider.EmployeeId is null)
        {
            return Task.FromResult(Result.Forbidden(employeeNotIdentifiedMessage));
        }

        return Task.FromResult(
            tenantProvider.TenantId.HasValue && organizationId == tenantProvider.TenantId.Value
                ? Result.Success()
                : Result.Forbidden(forbiddenMessage));
    }
}
