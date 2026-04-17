using FluentAssertions;
using Xunit;

namespace Bud.Api.UnitTests.Validators;

public sealed class PatchEmployeeValidatorTests
{
    private readonly PatchEmployeeValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidPartialRequest_ShouldPass()
    {
        var request = new PatchEmployeeRequest
        {
            FullName = "Colaborador Atualizado",
            Role = EmployeeRole.Leader
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_ShouldFail()
    {
        var request = new PatchEmployeeRequest
        {
            Email = "invalido"
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "E-mail deve ser válido.");
    }
}
