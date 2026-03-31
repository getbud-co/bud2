using Bud.Shared.Contracts;
using Bud.Domain.ValueObjects;
using FluentValidation;

namespace Bud.Api.Features.Organizations;

public sealed class CreateOrganizationValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O domínio é obrigatório.")
            .MaximumLength(200).WithMessage("O domínio não pode exceder 200 caracteres.")
            .Must(static name => string.IsNullOrWhiteSpace(name) || OrganizationDomainName.TryCreate(name, out _))
            .WithMessage("O nome deve ser um domínio válido (ex: empresa.com.br).");
    }
}

public sealed class PatchOrganizationValidator : AbstractValidator<PatchOrganizationRequest>
{
    public PatchOrganizationValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty().WithMessage("O domínio é obrigatório.")
            .MaximumLength(200).WithMessage("O domínio não pode exceder 200 caracteres.")
            .Must(static name => string.IsNullOrWhiteSpace(name) || OrganizationDomainName.TryCreate(name, out _))
            .WithMessage("O nome deve ser um domínio válido (ex: empresa.com.br).")
            .When(x => x.Name.HasValue);
    }
}
