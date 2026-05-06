using Bud.Shared.Contracts;
using FluentValidation;

namespace Bud.Api.Features.Indicators;

public sealed class CreateIndicatorValidator : AbstractValidator<CreateIndicatorRequest>
{
    public CreateIndicatorValidator()
    {
        RuleFor(x => x.MissionId)
            .NotEmpty().WithMessage("Meta é obrigatória.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Colaborador é obrigatório.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.MeasurementMode)
            .IsInEnum().WithMessage("Modo de medição inválido.");

        RuleFor(x => x.GoalType)
            .IsInEnum().WithMessage("Tipo de meta inválido.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Unidade inválida.");

        RuleFor(x => x.UnitLabel)
            .MaximumLength(50).WithMessage("Rótulo da unidade deve ter no máximo 50 caracteres.");

        When(x => x.GoalType == IndicatorGoalType.Reach || x.GoalType == IndicatorGoalType.Reduce, () =>
        {
            RuleFor(x => x.TargetValue)
                .NotNull().WithMessage("Valor alvo é obrigatório para este tipo de meta.");
        });

        When(x => x.GoalType == IndicatorGoalType.Above || x.GoalType == IndicatorGoalType.Between, () =>
        {
            RuleFor(x => x.LowThreshold)
                .NotNull().WithMessage("Limiar mínimo é obrigatório para este tipo de meta.")
                .GreaterThanOrEqualTo(0).WithMessage("Limiar mínimo deve ser maior ou igual a 0.");
        });

        When(x => x.GoalType == IndicatorGoalType.Below || x.GoalType == IndicatorGoalType.Between, () =>
        {
            RuleFor(x => x.HighThreshold)
                .NotNull().WithMessage("Limiar máximo é obrigatório para este tipo de meta.")
                .GreaterThanOrEqualTo(0).WithMessage("Limiar máximo deve ser maior ou igual a 0.");
        });
    }
}

public sealed class PatchIndicatorValidator : AbstractValidator<PatchIndicatorRequest>
{
    public PatchIndicatorValidator()
    {
        RuleFor(x => x.Title.Value)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.")
            .When(x => x.Title.HasValue);

        RuleFor(x => x.MeasurementMode.Value)
            .IsInEnum().WithMessage("Modo de medição inválido.")
            .When(x => x.MeasurementMode.HasValue);

        RuleFor(x => x.GoalType.Value)
            .IsInEnum().WithMessage("Tipo de meta inválido.")
            .When(x => x.GoalType.HasValue);

        RuleFor(x => x.Unit.Value)
            .IsInEnum().WithMessage("Unidade inválida.")
            .When(x => x.Unit.HasValue);

        When(x => x.GoalType == IndicatorGoalType.Reach || x.GoalType == IndicatorGoalType.Reduce, () =>
        {
            RuleFor(x => x.TargetValue)
                .NotNull().WithMessage("Valor alvo é obrigatório para este tipo de meta.");
        });

        When(x => x.GoalType == IndicatorGoalType.Above || x.GoalType == IndicatorGoalType.Between, () =>
        {
            RuleFor(x => x.LowThreshold.Value)
                .NotNull().WithMessage("Limiar mínimo é obrigatório para este tipo de meta.")
                .GreaterThanOrEqualTo(0).WithMessage("Limiar mínimo deve ser maior ou igual a 0.")
                .When(x => x.LowThreshold.HasValue);
        });

        When(x => x.GoalType == IndicatorGoalType.Below || x.GoalType == IndicatorGoalType.Between, () =>
        {
            RuleFor(x => x.HighThreshold.Value)
                .NotNull().WithMessage("Limiar máximo é obrigatório para este tipo de meta.")
                .GreaterThanOrEqualTo(0).WithMessage("Limiar máximo deve ser maior ou igual a 0.")
                .When(x => x.HighThreshold.HasValue);
        });
    }
}
