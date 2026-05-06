using FluentValidation;

namespace Bud.Api.Features.Tasks;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.MissionId)
            .NotEmpty().WithMessage("Meta é obrigatória.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Colaborador é obrigatório.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Descrição deve ter no máximo 2000 caracteres.")
            .When(x => x.Description is not null);
    }
}

public sealed class PatchTaskValidator : AbstractValidator<PatchTaskRequest>
{
    public PatchTaskValidator()
    {
        RuleFor(x => x.Title.Value)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.")
            .When(x => x.Title.HasValue);

        RuleFor(x => x.Description.Value)
            .MaximumLength(2000).WithMessage("Descrição deve ter no máximo 2000 caracteres.")
            .When(x => x.Description.HasValue && !string.IsNullOrEmpty(x.Description.Value));
    }
}
