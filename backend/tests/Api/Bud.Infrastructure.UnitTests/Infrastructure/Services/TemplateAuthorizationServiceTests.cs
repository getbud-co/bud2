using Bud.Application.Common;
using Bud.Application.Features.Templates;
using Bud.Domain.Templates;
using Bud.Infrastructure.Features.Templates;
using Bud.Infrastructure.UnitTests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Services;

public sealed class TemplateAuthorizationServiceTests
{
    [Fact]
    public async Task EvaluateWriteAsync_WhenEmployeeMissing_ReturnsForbidden()
    {
        var repository = new Mock<ITemplateRepository>(MockBehavior.Strict);
        var tenantProvider = new TestTenantProvider { EmployeeId = null };
        var service = new TemplateAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<TemplateResource>)service)
            .EvaluateAsync(new TemplateResource(Guid.NewGuid()));

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Error.Should().Be("Colaborador não identificado.");
    }

    [Fact]
    public async Task EvaluateWriteAsync_WhenTenantMatches_ReturnsSuccess()
    {
        var organizationId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var repository = new Mock<ITemplateRepository>();
        repository
            .Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Template { Id = templateId, OrganizationId = organizationId });

        var tenantProvider = new TestTenantProvider
        {
            TenantId = organizationId,
            EmployeeId = Guid.NewGuid()
        };
        var service = new TemplateAuthorizationService(repository.Object, tenantProvider);

        var result = await ((IWriteAuthorizationRule<TemplateResource>)service)
            .EvaluateAsync(new TemplateResource(templateId));

        result.IsSuccess.Should().BeTrue();
    }
}
