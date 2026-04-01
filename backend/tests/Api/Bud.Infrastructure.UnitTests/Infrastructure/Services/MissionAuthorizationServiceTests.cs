using Bud.Application.Common;
using Bud.Application.Features.Missions;
using Bud.Domain.Missions;
using Bud.Infrastructure.Features.Missions;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public sealed class MissionAuthorizationServiceTests
{
    [Fact]
    public async Task EvaluateWriteAsync_WhenEmployeeMissing_ReturnsForbidden()
    {
        var repository = new Mock<IMissionRepository>(MockBehavior.Strict);
        var tenantProvider = new TestTenantProvider { EmployeeId = null };
        var service = new MissionAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<MissionResource>)service)
            .EvaluateAsync(new MissionResource(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Colaborador não identificado.");
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenTenantMatches_ReturnsSuccess()
    {
        var organizationId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var repository = new Mock<IMissionRepository>();
        repository
            .Setup(r => r.GetByIdReadOnlyAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mission { Id = missionId, OrganizationId = organizationId });

        var tenantProvider = new TestTenantProvider
        {
            TenantId = organizationId,
            EmployeeId = Guid.NewGuid()
        };
        var service = new MissionAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<MissionResource>)service)
            .EvaluateAsync(new MissionResource(missionId));

        result.IsSuccess.Should().BeTrue();
    }
}
