using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Api.UnitTests.Validators;

public sealed class CreateEmployeeValidatorTests
{
    [Fact]
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.IsEmailUniqueAsync("colab@bud.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new CreateEmployeeValidator(repository.Object);
        var request = new CreateEmployeeRequest
        {
            FullName = "Colaborador Bud",
            Email = "colab@bud.com",
            Role = EmployeeRole.Leader
        };

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithDuplicateEmail_ShouldFail()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.IsEmailUniqueAsync("duplicado@bud.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new CreateEmployeeValidator(repository.Object);
        var request = new CreateEmployeeRequest
        {
            FullName = "Colaborador Bud",
            Email = "duplicado@bud.com",
            Role = EmployeeRole.IndividualContributor
        };

        var result = await validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "E-mail já está em uso.");
    }
}
