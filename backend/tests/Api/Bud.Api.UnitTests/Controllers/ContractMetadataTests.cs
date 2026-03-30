using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Bud.Api.UnitTests.Controllers;

public sealed class ContractMetadataTests
{
    [Fact]
    public void OrganizationsCreate_ShouldNotDocumentNotFound()
    {
        var statuses = GetDocumentedStatuses(typeof(Bud.Api.Features.Organizations.OrganizationsController), "Create");

        statuses.Should().BeEquivalentTo([201, 400, 403, 409]);
    }

    [Fact]
    public void MissionsCreate_ShouldDocumentForbiddenAndNotFound()
    {
        var statuses = GetDocumentedStatuses(typeof(Bud.Api.Features.Missions.MissionsController), "Create");

        statuses.Should().BeEquivalentTo([201, 400, 403, 404]);
    }

    [Fact]
    public void EmployeesCreate_ShouldDocumentTeamNotFound()
    {
        var statuses = GetDocumentedStatuses(typeof(Bud.Api.Features.Employees.EmployeesController), "Create");

        statuses.Should().BeEquivalentTo([201, 400, 403, 404]);
    }

    [Fact]
    public void TasksCreate_ShouldDocumentCreatedAgainstCanonicalGet()
    {
        var statuses = GetDocumentedStatuses(typeof(Bud.Api.Features.Tasks.TasksController), "Create");

        statuses.Should().BeEquivalentTo([201, 400, 403, 404]);
    }

    [Fact]
    public void TasksGetById_ShouldDocumentOkAndNotFound()
    {
        var statuses = GetDocumentedStatuses(typeof(Bud.Api.Features.Tasks.TasksController), "GetById");

        statuses.Should().BeEquivalentTo([200, 404]);
    }

    private static int[] GetDocumentedStatuses(Type controllerType, string methodName)
    {
        var method = controllerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        method.Should().NotBeNull();

        return method!
            .GetCustomAttributes<ProducesResponseTypeAttribute>()
            .Select(attribute => attribute.StatusCode)
            .OrderBy(status => status)
            .ToArray();
    }
}
