using Bud.Application.Common;

namespace Bud.Application.Ports;

public interface IReadAuthorizationRule<in TResource>
{
    Task<Result> EvaluateAsync(TResource resource, CancellationToken cancellationToken = default);
}
