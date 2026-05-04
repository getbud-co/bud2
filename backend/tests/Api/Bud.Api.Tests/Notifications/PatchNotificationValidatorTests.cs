using FluentAssertions;
using Xunit;

namespace Bud.Api.Tests.Notifications;

public sealed class PatchNotificationValidatorTests
{
    private readonly PatchNotificationValidator _validator = new();

    [Fact]
    public async Task Validate_WhenIsReadIsTrue_ShouldPass()
    {
        var request = new PatchNotificationRequest
        {
            IsRead = true
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenIsReadMissing_ShouldFail()
    {
        var request = new PatchNotificationRequest();

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "O campo 'isRead' é obrigatório.");
    }

    [Fact]
    public async Task Validate_WhenIsReadIsFalse_ShouldFail()
    {
        var request = new PatchNotificationRequest
        {
            IsRead = false
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Atualmente apenas a marcação como lida é suportada.");
    }
}
