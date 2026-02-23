using Bud.Server.Application.ReadModels;
using Bud.Server.Application.Common;

namespace Bud.Server.Application.Ports;

public interface IAuthService
{
    Task<Result<LoginResult>> LoginAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<Bud.Server.Application.ReadModels.OrganizationSnapshot>>> GetMyOrganizationsAsync(string email, CancellationToken cancellationToken = default);
}
