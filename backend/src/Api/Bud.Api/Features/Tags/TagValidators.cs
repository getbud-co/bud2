using FluentValidation;

namespace Bud.Api.Features.Tags;

public sealed class CreateTagValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Color)
            .IsInEnum().WithMessage("Cor inválida.");
    }
}

public sealed class PatchTagValidator : AbstractValidator<PatchTagRequest>
{
    public PatchTagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Color)
            .IsInEnum().WithMessage("Cor inválida.");
    }
}
