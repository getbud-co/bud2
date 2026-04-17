using Bud.Domain.ValueObjects;
using FluentValidation;

namespace Bud.Api.Features.Sessions;

public sealed class CreateSessionValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(email => EmailAddress.TryCreate(email, out _))
            .WithMessage("Informe um e-mail válido.");
    }
}
