using System.Text.Json;
using Bud.Mcp.Tests.Helpers;

namespace Bud.Mcp.Tests.Tools;

public sealed class McpToolServiceTests
{
    [Fact]
    public void GetTools_ShouldExposePlatformActionsOnly()
    {
        var service = CreateService();

        service.GetTools().Select(t => t.Name).Should().BeEquivalentTo(
            "auth_login",
            "auth_whoami",
            "tenant_list_available",
            "tenant_set_current",
            "help_list_actions",
            "help_action_schema");
    }

    [Fact]
    public async Task ExecuteAsync_AuthLoginWithoutEmail_ShouldFailWithPortugueseMessage()
    {
        var service = CreateService();
        using var doc = JsonDocument.Parse("{}");

        var act = () => service.ExecuteAsync("auth_login", doc.RootElement);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Parâmetro obrigatório ausente: email.");
    }

    [Fact]
    public async Task ExecuteAsync_HelpActionSchemaWithoutAction_ShouldReturnAllActions()
    {
        var service = CreateService();
        using var doc = JsonDocument.Parse("{}");

        var result = await service.ExecuteAsync("help_action_schema", doc.RootElement);

        result["actions"]!.AsArray()
            .Select(item => item!["name"]!.GetValue<string>())
            .Should().Contain("auth_login");
    }

    [Fact]
    public async Task ExecuteAsync_TenantSetCurrentWithoutTenantId_ShouldFailWithPortugueseMessage()
    {
        var service = CreateService();
        using var doc = JsonDocument.Parse("{}");

        var act = () => service.ExecuteAsync("tenant_set_current", doc.RootElement);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Parâmetro obrigatório ausente: tenantId.");
    }

    private static McpToolService CreateService()
    {
        var handler = new StubHttpMessageHandler(_ => throw new InvalidOperationException("Request inesperada."));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://bud.test") };
        var options = new BudMcpOptions("http://bud.test", null, null, 30, 30);
        var session = new BudApiSession(httpClient, options);
        return new McpToolService(session);
    }
}
