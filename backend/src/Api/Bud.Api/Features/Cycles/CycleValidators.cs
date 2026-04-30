using FluentValidation;

namespace Bud.Api.Features.Cycles;

public sealed class CreateCycleValidator : AbstractValidator<CreateCycleRequest>
{
    public CreateCycleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Cadence)
            .IsInEnum().WithMessage("Cadência inválida.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Data de término é obrigatória.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Data de término deve ser igual ou posterior à data de início.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido.");
    }
}

public sealed class PatchCycleValidator : AbstractValidator<PatchCycleRequest>
{
    public PatchCycleValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.")
            .When(x => x.Name.HasValue);

        RuleFor(x => x.Cadence.Value)
            .IsInEnum().WithMessage("Cadência inválida.")
            .When(x => x.Cadence.HasValue);

        RuleFor(x => x.StartDate.Value)
            .NotEmpty().WithMessage("Data de início é obrigatória.")
            .When(x => x.StartDate.HasValue);

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
