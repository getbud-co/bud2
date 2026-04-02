using Bud.Application.Common;

namespace Bud.Application.Features.Templates.UseCases;

public sealed class GetTemplateById(
    ITemplateRepository templateRepository)
{
    public async Task<Result<Template>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await templateRepository.GetByIdReadOnlyAsync(id, cancellationToken);
        if (template is null)
        {
            return Result<Template>.NotFound(UserErrorMessages.TemplateNotFound);
        }

        return Result<Template>.Success(template);
    }
}
