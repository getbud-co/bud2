using Bud.Shared.Contracts;
using FluentValidation;

namespace Bud.Api.Features.Organizations;

public sealed class CreateOrganizationValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da organização é obrigatório.")
            .MaximumLength(200).WithMessage("O nome não pode exceder 200 caracteres.");
    }
}

public sealed class PatchOrganizationValidator : AbstractValidator<PatchOrganizationRequest>
{
    public PatchOrganizationValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty().WithMessage("O nome da organização é obrigatório.")
            .MaximumLength(200).WithMessage("O nome não pode exceder 200 caracteres.")
            .When(x => x.Name.HasValue);
    }
}
