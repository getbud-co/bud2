using FluentValidation;

namespace Bud.Server.Validators;

public sealed class CreateMissionValidator : AbstractValidator<CreateMissionRequest>
{
    public CreateMissionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Data de término é obrigatória.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Data de término deve ser igual ou posterior à data de início.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido.");

        RuleFor(x => x.ScopeType)
            .IsInEnum().WithMessage("Tipo de escopo inválido.");

        RuleFor(x => x.ScopeId)
            .NotEmpty().WithMessage("Escopo é obrigatório.");
    }
}

public sealed class PatchMissionValidator : AbstractValidator<PatchMissionRequest>
{
    public PatchMissionValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.")
            .When(x => x.Name.HasValue);

        RuleFor(x => x.StartDate.Value)
            .NotEmpty().WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.EndDate.Value)
            .NotEmpty().WithMessage("Data de término é obrigatória.")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Status.Value)
            .IsInEnum().WithMessage("Status inválido.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x)
            .Must(x =>
            {
                if (x.StartDate.HasValue && x.EndDate.HasValue)
                {
                    return x.EndDate.Value >= x.StartDate.Value;
                }

                return true;
            })
            .WithMessage("Data de término deve ser igual ou posterior à data de início.");
    }
}
