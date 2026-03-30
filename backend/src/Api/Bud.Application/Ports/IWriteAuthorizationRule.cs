using Bud.Application.Common;

namespace Bud.Application.Ports;

public interface IWriteAuthorizationRule<in TResource>
{
    Task<Result> EvaluateAsync(TResource resource, CancellationToken cancellationToken = default);
}
