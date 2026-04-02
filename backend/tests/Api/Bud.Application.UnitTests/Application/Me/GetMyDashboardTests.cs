using FluentAssertions;
using Moq;
using Xunit;
using Bud.Application.Common;

namespace Bud.Application.UnitTests.Application.Me;

public sealed class GetMyDashboardTests
{
    [Fact]
    public async Task ExecuteAsync_WithoutEmployeeInContext_ReturnsForbidden()
    {
        var repository = new Mock<IMyDashboardReadStore>();
        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(x => x.EmployeeId).Returns((Guid?)null);

        var useCase = new GetMyDashboard(repository.Object, tenantProvider.Object);

        var result = await useCase.ExecuteAsync(null);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Funcionário não identificado.");
        repository.Verify(r => r.GetMyDashboardAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DelegatesToRepositoryUsingAuthenticatedEmployee()
    {
        var employeeId = Guid.NewGuid();
        var repository = new Mock<IMyDashboardReadStore>();
        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(x => x.EmployeeId).Returns(employeeId);
        repository
            .Setup(r => r.GetMyDashboardAsync(employeeId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardSnapshot());

        var useCase = new GetMyDashboard(repository.Object, tenantProvider.Object);

        var result = await useCase.ExecuteAsync(null);

        result.IsSuccess.Should().BeTrue();
        repository.Verify(r => r.GetMyDashboardAsync(employeeId, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_RepositoryReturnsNull_ReturnsNotFound()
    {
        var employeeId = Guid.NewGuid();
        var repository = new Mock<IMyDashboardReadStore>();
        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(x => x.EmployeeId).Returns(employeeId);
        repository
            .Setup(r => r.GetMyDashboardAsync(employeeId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DashboardSnapshot?)null);

        var useCase = new GetMyDashboard(repository.Object, tenantProvider.Object);

        var result = await useCase.ExecuteAsync(null);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Funcionário não encontrado.");
    }

    [Fact]
    public async Task ExecuteAsync_WithTeamId_PassesTeamIdToRepository()
    {
        var employeeId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var repository = new Mock<IMyDashboardReadStore>();
        var tenantProvider = new Mock<ITenantProvider>();
        tenantProvider.SetupGet(x => x.EmployeeId).Returns(employeeId);
        repository
            .Setup(r => r.GetMyDashboardAsync(employeeId, teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardSnapshot());

        var useCase = new GetMyDashboard(repository.Object, tenantProvider.Object);

        var result = await useCase.ExecuteAsync(teamId);

        result.IsSuccess.Should().BeTrue();
        repository.Verify(r => r.GetMyDashboardAsync(employeeId, teamId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
