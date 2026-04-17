using FluentAssertions;
using Xunit;

namespace Bud.Api.UnitTests.Validators;

public sealed class CreateEmployeeValidatorTests
{
    [Fact]
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        var validator = new CreateEmployeeValidator();
        var request = new CreateEmployeeRequest
        {
            FullName = "Colaborador Bud",
            Email = "colab@bud.com",
            Role = EmployeeRole.Leader
        };

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("missing-at-sign")]
    [InlineData("user@local")]
    public async Task Validate_WithInvalidEmail_ShouldFail(string email)
    {
        var validator = new CreateEmployeeValidator();
        var request = new CreateEmployeeRequest
        {
            FullName = "Colaborador Bud",
            Email = email,
            Role = EmployeeRole.Leader
        };

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "E-mail deve ser válido.");
    }
}
