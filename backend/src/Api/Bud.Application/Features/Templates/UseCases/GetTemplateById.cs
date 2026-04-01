using System.Security.Claims;
using Bud.Application.Common;
using Bud.Application.Ports;

namespace Bud.Application.Features.Templates.UseCases;

public sealed class GetTemplateById(
    ITemplateRepository templateRepository,
    IApplicationAuthorizationGateway authorizationGateway)
{
    public async Task<Result<Template>> ExecuteAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken = default)
    {
        var template = await templateRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (template is null)
        {
            return Result<Template>.NotFound(UserErrorMessages.TemplateNotFound);
        }

        var canRead = await authorizationGateway.CanReadAsync(user, new TemplateResource(id), cancellationToken);
        if (!canRead)
        {
            return Result<Template>.Forbidden(UserErrorMessages.TemplateNotFound);
        }

        return Result<Template>.Success(template);
    }
}
