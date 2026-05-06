using FluentValidation;

namespace Bud.Api.Features.Missions;

public sealed class CreateMissionValidator : AbstractValidator<CreateMissionRequest>
{
    public CreateMissionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Dimension)
            .MaximumLength(100).WithMessage("Dimensão deve ter no máximo 100 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Dimension));

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido.");

        RuleFor(x => x.Visibility)
            .IsInEnum().WithMessage("Visibilidade inválida.");
    }
}

public sealed class PatchMissionValidator : AbstractValidator<PatchMissionRequest>
{
    public PatchMissionValidator()
    {
        RuleFor(x => x.Title.Value)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.")
            .When(x => x.Title.HasValue);

        RuleFor(x => x.Description.Value)
            .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres.")
            .When(x => x.Description.HasValue && !string.IsNullOrEmpty(x.Description.Value));

        RuleFor(x => x.Dimension.Value)
            .MaximumLength(100).WithMessage("Dimensão deve ter no máximo 100 caracteres.")
            .When(x => x.Dimension.HasValue && !string.IsNullOrEmpty(x.Dimension.Value));

        RuleFor(x => x.Status.Value)
            .IsInEnum().WithMessage("Status inválido.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Visibility.Value)
            .IsInEnum().WithMessage("Visibilidade inválida.")
            .When(x => x.Visibility.HasValue);
    }
}
