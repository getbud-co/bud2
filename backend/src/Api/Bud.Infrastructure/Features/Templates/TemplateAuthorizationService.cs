using Bud.Application.Common;
using Bud.Application.Features.Templates;
using Bud.Application.Ports;
using Bud.Infrastructure.Authorization;

namespace Bud.Infrastructure.Features.Templates;

public sealed class TemplateAuthorizationService(
    ITemplateRepository templateRepository,
    ITenantProvider tenantProvider)
    : IReadAuthorizationRule<TemplateResource>,
      IWriteAuthorizationRule<TemplateResource>,
      IWriteAuthorizationRule<CreateTemplateContext>
{
    public async Task<Result> EvaluateAsync(TemplateResource resource, CancellationToken cancellationToken = default)
        => await TenantScopedAuthorization.AuthorizeReadAsync(
            tenantProvider,
            ct => templateRepository.GetByIdReadOnlyAsync(resource.TemplateId, ct),
            template => template.OrganizationId,
            "Template não encontrado.",
            "Você não tem permissão para acessar este template.",
            cancellationToken);

    async Task<Result> IWriteAuthorizationRule<TemplateResource>.EvaluateAsync(TemplateResource resource, CancellationToken cancellationToken)
        => await TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            ct => templateRepository.GetByIdAsync(resource.TemplateId, ct),
            template => template.OrganizationId,
            "Template não encontrado.",
            "Colaborador não identificado.",
            "Você não tem permissão para atualizar templates nesta organização.",
            cancellationToken);

    Task<Result> IWriteAuthorizationRule<CreateTemplateContext>.EvaluateAsync(CreateTemplateContext context, CancellationToken cancellationToken)
        => TenantScopedAuthorization.AuthorizeWriteAsync(
            tenantProvider,
            context.OrganizationId,
            "Colaborador não identificado.",
            "Você não tem permissão para criar templates nesta organização.");
}
