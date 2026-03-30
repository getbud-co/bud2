using System.Reflection;
using Bud.Application.Common;
using Bud.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace Bud.Api.Authorization.Handlers;

internal static class AuthorizationRuleInvoker
{
    private static readonly MethodInfo InvokeReadMethod =
        typeof(AuthorizationRuleInvoker).GetMethod(nameof(InvokeReadAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo InvokeWriteMethod =
        typeof(AuthorizationRuleInvoker).GetMethod(nameof(InvokeWriteAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static async Task<Result?> TryInvokeReadAsync(IServiceProvider serviceProvider, object resource, CancellationToken cancellationToken)
    {
        var task = (Task<Result?>)InvokeReadMethod.MakeGenericMethod(resource.GetType())
            .Invoke(null, [serviceProvider, resource, cancellationToken])!;

        return await task;
    }

    public static async Task<Result?> TryInvokeWriteAsync(IServiceProvider serviceProvider, object resource, CancellationToken cancellationToken)
    {
        var task = (Task<Result?>)InvokeWriteMethod.MakeGenericMethod(resource.GetType())
            .Invoke(null, [serviceProvider, resource, cancellationToken])!;

        return await task;
    }

    private static async Task<Result?> InvokeReadAsync<TResource>(IServiceProvider serviceProvider, object resource, CancellationToken cancellationToken)
    {
        var rule = serviceProvider.GetService<IReadAuthorizationRule<TResource>>();
        if (rule is null)
        {
            return null;
        }

        return await rule.EvaluateAsync((TResource)resource, cancellationToken);
    }

    private static async Task<Result?> InvokeWriteAsync<TResource>(IServiceProvider serviceProvider, object resource, CancellationToken cancellationToken)
    {
        var rule = serviceProvider.GetService<IWriteAuthorizationRule<TResource>>();
        if (rule is null)
        {
            return null;
        }

        return await rule.EvaluateAsync((TResource)resource, cancellationToken);
    }
}
