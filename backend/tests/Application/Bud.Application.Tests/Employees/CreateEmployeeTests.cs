namespace Bud.Application.Tests.Employees;

public sealed class CreateEmployeeTests
{
    [Fact]
    public async Task ExecuteAsync_WhenEmailAlreadyExists_ShouldReturnValidationFailure()
    {
        var repository = new Mock<IEmployeeRepository>();
        repository
            .Setup(x => x.IsEmailUniqueAsync(It.Is<EmailAddress>(email => email.Value == "duplicado@bud.com"), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var tenantProvider = new TestTenantProvider { TenantId = Guid.NewGuid() };
        var sut = new CreateEmployee(repository.Object, tenantProvider, NullLogger<CreateEmployee>.Instance);

        var result = await sut.ExecuteAsync(new CreateEmployeeCommand(
            "Colaborador Bud",
            "  DUPLICADO@bud.com ",
            EmployeeRole.Leader));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("E-mail já está em uso.");
    }
}
