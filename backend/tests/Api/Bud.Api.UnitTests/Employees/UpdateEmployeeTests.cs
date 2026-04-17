using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bud.Api.UnitTests.Employees;

public sealed class UpdateEmployeeTests
{
    [Fact]
    public async Task ExecuteAsync_WhenEmailAlreadyExists_ShouldReturnValidationFailure()
    {
        var employee = Employee.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EmployeeName.Create("Maria Silva"),
            EmailAddress.Create("maria@bud.com"),
            EmployeeRole.Leader);

        var repository = new Mock<IEmployeeRepository>();
        repository.Setup(x => x.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        repository
            .Setup(x => x.IsEmailUniqueAsync(It.Is<EmailAddress>(email => email.Value == "duplicado@bud.com"), employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new UpdateEmployee(repository.Object, NullLogger<UpdateEmployee>.Instance);

        var result = await sut.ExecuteAsync(
            employee.Id,
            new UpdateEmployeeCommand("Maria Silva", "duplicado@bud.com", EmployeeRole.Leader));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Error.Should().Be("E-mail já está em uso.");
    }
}
