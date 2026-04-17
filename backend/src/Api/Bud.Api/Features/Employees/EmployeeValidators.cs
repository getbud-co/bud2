using FluentValidation;

namespace Bud.Api.Features.Employees;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator(IEmployeeRepository employeeRepository)
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .MaximumLength(320).WithMessage("E-mail deve ter no máximo 320 caracteres.")
            .EmailAddress().WithMessage("E-mail deve ser válido.")
            .MustAsync((email, cancellationToken) => employeeRepository.IsEmailUniqueAsync(email, null, cancellationToken))
            .WithMessage("E-mail já está em uso.");

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
            .When(x => x.FullName.HasValue);

        RuleFor(x => x.Email.Value)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .MaximumLength(320).WithMessage("E-mail deve ter no máximo 320 caracteres.")
            .EmailAddress().WithMessage("E-mail deve ser válido.")
            .When(x => x.Email.HasValue);

        RuleFor(x => x.Role.Value)
            .IsInEnum().WithMessage("Função inválida.")
            .When(x => x.Role.HasValue);
    }
}
