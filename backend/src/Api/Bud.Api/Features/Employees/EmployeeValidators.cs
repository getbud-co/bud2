using Bud.Application.Ports;
using Bud.Shared.Contracts;
using FluentValidation;

namespace Bud.Api.Features.Employees;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    private readonly IMemberRepository _employeeRepository;
    private readonly ITenantProvider _tenantProvider;

    public CreateEmployeeValidator(IMemberRepository employeeRepository, ITenantProvider tenantProvider)
    {
        _employeeRepository = employeeRepository;
        _tenantProvider = tenantProvider;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .MaximumLength(320).WithMessage("E-mail deve ter no máximo 320 caracteres.")
            .EmailAddress().WithMessage("E-mail deve ser válido.")
            .MustAsync(BeUniqueEmail).WithMessage("E-mail já está em uso.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Função inválida.");

        RuleFor(x => x.LeaderId)
            .MustAsync(BeValidLeader).WithMessage("O líder deve existir, pertencer à mesma organização e ter a função de Líder.")
            .When(x => x.LeaderId.HasValue);
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return await _employeeRepository.IsEmailUniqueAsync(email, null, cancellationToken);
    }

    private async Task<bool> BeValidLeader(CreateEmployeeRequest _, Guid? leaderId, CancellationToken cancellationToken)
    {
        if (!leaderId.HasValue)
        {
            return true;
        }

        return await _employeeRepository.IsValidLeaderAsync(leaderId.Value, _tenantProvider.TenantId, cancellationToken);
    }
}

public sealed class PatchEmployeeValidator : AbstractValidator<PatchEmployeeRequest>
{
    private readonly IMemberRepository _employeeRepository;
    private readonly ITenantProvider _tenantProvider;

    public PatchEmployeeValidator(IMemberRepository employeeRepository, ITenantProvider tenantProvider)
    {
        _employeeRepository = employeeRepository;
        _tenantProvider = tenantProvider;

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

        RuleFor(x => x.LeaderId.Value)
            .MustAsync(BeValidLeaderForUpdate).WithMessage("O líder deve existir, pertencer à mesma organização e ter a função de Líder.")
            .When(x => x.LeaderId.HasValue);
    }

    private async Task<bool> BeValidLeaderForUpdate(Guid? leaderId, CancellationToken cancellationToken)
    {
        if (!leaderId.HasValue)
        {
            return true;
        }

        return await _employeeRepository.IsValidLeaderAsync(leaderId.Value, _tenantProvider.TenantId, cancellationToken);
    }
}
