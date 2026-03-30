using Bud.Shared.Contracts;
using FluentValidation;

namespace Bud.Api.Features.Templates;

public sealed class CreateTemplateValidator : AbstractValidator<CreateTemplateRequest>
{
    public CreateTemplateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.MissionNamePattern)
            .MaximumLength(200).WithMessage("Padrão de nome deve ter no máximo 200 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.MissionNamePattern));

        RuleFor(x => x.MissionDescriptionPattern)
            .MaximumLength(1000).WithMessage("Padrão de descrição deve ter no máximo 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.MissionDescriptionPattern));

        RuleForEach(x => x.Missions)
            .SetValidator(new TemplateMissionDtoValidator());

        RuleForEach(x => x.Indicators)
            .SetValidator(new TemplateIndicatorDtoValidator());

        RuleFor(x => x)
            .Must(HaveValidMissionReferences)
            .WithMessage("Um ou mais indicadores referenciam metas inexistentes no template.");
    }

    private static bool HaveValidMissionReferences(CreateTemplateRequest request)
    {
        var missionIds = request.Missions
            .Where(g => g.Id.HasValue)
            .Select(g => g.Id!.Value)
            .ToHashSet();

        return request.Indicators.All(indicator =>
            indicator.TemplateMissionId is null || missionIds.Contains(indicator.TemplateMissionId.Value));
    }
}

public sealed class PatchTemplateValidator : AbstractValidator<PatchTemplateRequest>
{
    public PatchTemplateValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.")
            .When(x => x.Name.HasValue);

        RuleFor(x => x.Description.Value)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.")
            .When(x => x.Description.HasValue && !string.IsNullOrEmpty(x.Description.Value));

        RuleFor(x => x.MissionNamePattern.Value)
            .MaximumLength(200).WithMessage("Padrão de nome deve ter no máximo 200 caracteres.")
            .When(x => x.MissionNamePattern.HasValue && !string.IsNullOrEmpty(x.MissionNamePattern.Value));

        RuleFor(x => x.MissionDescriptionPattern.Value)
            .MaximumLength(1000).WithMessage("Padrão de descrição deve ter no máximo 1000 caracteres.")
            .When(x => x.MissionDescriptionPattern.HasValue && !string.IsNullOrEmpty(x.MissionDescriptionPattern.Value));

        When(x => x.Missions.HasValue && x.Missions.Value is not null, () =>
        {
            RuleForEach(x => x.Missions.Value!)
                .SetValidator(new TemplateMissionDtoValidator());
        });

        When(x => x.Indicators.HasValue && x.Indicators.Value is not null, () =>
        {
            RuleForEach(x => x.Indicators.Value!)
                .SetValidator(new TemplateIndicatorDtoValidator());
        });

        RuleFor(x => x)
            .Must(HaveValidMissionReferences)
            .WithMessage("Um ou mais indicadores referenciam metas inexistentes no template.");
    }

    private static bool HaveValidMissionReferences(PatchTemplateRequest request)
    {
        var missionIds = request.Missions.AsEnumerable()
            .Where(g => g.Id.HasValue)
            .Select(g => g.Id!.Value)
            .ToHashSet();

        return request.Indicators.AsEnumerable().All(indicator =>
            indicator.TemplateMissionId is null || missionIds.Contains(indicator.TemplateMissionId.Value));
    }
}

public sealed class TemplateMissionDtoValidator : AbstractValidator<TemplateMissionRequest>
{
    public TemplateMissionDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Dimension)
            .MaximumLength(100).WithMessage("Dimensão deve ter no máximo 100 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Dimension));

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Índice de ordenação deve ser maior ou igual a 0.");
    }
}

public sealed class TemplateIndicatorDtoValidator : AbstractValidator<TemplateIndicatorRequest>
{
    public TemplateIndicatorDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo inválido.");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Índice de ordenação deve ser maior ou igual a 0.");

        When(x => x.Type == IndicatorType.Qualitative, () =>
        {
            RuleFor(x => x.TargetText)
                .MaximumLength(1000).WithMessage("Texto alvo deve ter no máximo 1000 caracteres.");
        });

        When(x => x.Type == IndicatorType.Quantitative, () =>
        {
            RuleFor(x => x.QuantitativeType)
                .NotNull().WithMessage("Tipo quantitativo é obrigatório para indicadores quantitativos.")
                .IsInEnum().WithMessage("Tipo quantitativo inválido.");

            RuleFor(x => x.Unit)
                .NotNull().WithMessage("Unidade é obrigatória para indicadores quantitativos.")
                .IsInEnum().WithMessage("Unidade inválida.");

            When(x => x.QuantitativeType == QuantitativeIndicatorType.KeepAbove, () =>
            {
                RuleFor(x => x.MinValue)
                    .NotNull().WithMessage("Valor mínimo é obrigatório para indicadores KeepAbove.")
                    .GreaterThanOrEqualTo(0).WithMessage("Valor mínimo deve ser maior ou igual a 0.");
            });

            When(x => x.QuantitativeType == QuantitativeIndicatorType.KeepBelow, () =>
            {
                RuleFor(x => x.MaxValue)
                    .NotNull().WithMessage("Valor máximo é obrigatório para indicadores KeepBelow.")
                    .GreaterThanOrEqualTo(0).WithMessage("Valor máximo deve ser maior ou igual a 0.");
            });

            When(x => x.QuantitativeType == QuantitativeIndicatorType.KeepBetween, () =>
            {
                RuleFor(x => x.MinValue)
                    .NotNull().WithMessage("Valor mínimo é obrigatório para indicadores KeepBetween.")
                    .GreaterThanOrEqualTo(0).WithMessage("Valor mínimo deve ser maior ou igual a 0.");

                RuleFor(x => x.MaxValue)
                    .NotNull().WithMessage("Valor máximo é obrigatório para indicadores KeepBetween.")
                    .GreaterThanOrEqualTo(0).WithMessage("Valor máximo deve ser maior ou igual a 0.")
                    .GreaterThan(x => x.MinValue ?? 0).WithMessage("Valor máximo deve ser maior que o valor mínimo.");
            });

            When(x => x.QuantitativeType == QuantitativeIndicatorType.Achieve, () =>
            {
                RuleFor(x => x.MaxValue)
                    .NotNull().WithMessage("Valor máximo é obrigatório para indicadores Achieve.")
                    .GreaterThanOrEqualTo(0).WithMessage("Valor máximo deve ser maior ou igual a 0.");
            });

            When(x => x.QuantitativeType == QuantitativeIndicatorType.Reduce, () =>
            {
                RuleFor(x => x.MaxValue)
                    .NotNull().WithMessage("Valor máximo é obrigatório para indicadores Reduce.")
                    .GreaterThanOrEqualTo(0).WithMessage("Valor máximo deve ser maior ou igual a 0.");
            });
        });
    }
}
