using FluentValidation;

namespace Bud.Api.Features.Notifications;

public sealed class PatchNotificationValidator : AbstractValidator<PatchNotificationRequest>
{
    public PatchNotificationValidator()
    {
        RuleFor(x => x.IsRead.HasValue)
            .Equal(true)
            .WithMessage("O campo 'isRead' é obrigatório.");

        RuleFor(x => x.IsRead.Value)
            .Equal(true)
            .When(x => x.IsRead.HasValue)
            .WithMessage("Atualmente apenas a marcação como lida é suportada.");
    }
}
