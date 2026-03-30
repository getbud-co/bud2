using Bud.Application.Common;
using Bud.Application.Features.Indicators;
using Bud.Domain.Indicators;
using Bud.Infrastructure.Features.Indicators;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public sealed class IndicatorAuthorizationServiceTests
{
    [Fact]
    public async Task EvaluateWriteAsync_WhenIndicatorMissing_ReturnsNotFound()
    {
        var repository = new Mock<IIndicatorRepository>();
        repository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Indicator?)null);

        var tenantProvider = new TestTenantProvider { EmployeeId = Guid.NewGuid() };
        var service = new IndicatorAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<IndicatorResource>)service)
            .EvaluateAsync(new IndicatorResource(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Error.Should().Be("Indicador não encontrado.");
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenTenantMatches_ReturnsSuccess()
    {
        var organizationId = Guid.NewGuid();
        var indicatorId = Guid.NewGuid();
        var repository = new Mock<IIndicatorRepository>();
        repository
            .Setup(r => r.GetByIdAsync(indicatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Indicator { Id = indicatorId, OrganizationId = organizationId });

        var tenantProvider = new TestTenantProvider
        {
            TenantId = organizationId,
            EmployeeId = Guid.NewGuid()
        };
        var service = new IndicatorAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<IndicatorResource>)service)
            .EvaluateAsync(new IndicatorResource(indicatorId));

        result.IsSuccess.Should().BeTrue();
    }
}
