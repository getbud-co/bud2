using Bud.Domain.ValueObjects;
using FluentValidation;

namespace Bud.Api.Features.Employees;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres.")
            .Must(static fullName => EmployeeName.TryCreate(fullName, out _)).WithMessage("Nome completo é obrigatório.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .MaximumLength(320).WithMessage("E-mail deve ter no máximo 320 caracteres.")
            .Must(static email => EmailAddress.TryCreate(email, out _)).WithMessage("E-mail deve ser válido.")
            ;

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Função inválida.");
    }
}

public sealed class PatchEmployeeValidator : AbstractValidator<PatchEmployeeRequest>
{
    public PatchEmployeeValidator()
    {
        RuleFor(x => x.FullName.Value)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres.")
            .Must(static fullName => EmployeeName.TryCreate(fullName, out _)).WithMessage("Nome completo é obrigatório.")
            .When(x => x.FullName.HasValue);

        RuleFor(x => x.Email.Value)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .MaximumLength(320).WithMessage("E-mail deve ter no máximo 320 caracteres.")
            .Must(static email => EmailAddress.TryCreate(email, out _)).WithMessage("E-mail deve ser válido.")
            .When(x => x.Email.HasValue);

        RuleFor(x => x.Role.Value)
            .IsInEnum().WithMessage("Função inválida.")
            .When(x => x.Role.HasValue);
    }
}
