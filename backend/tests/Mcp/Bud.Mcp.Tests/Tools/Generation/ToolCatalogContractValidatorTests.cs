using System.Text.Json.Nodes;
using Bud.Mcp.Tools;
using Bud.Mcp.Tools.Generation;

namespace Bud.Mcp.Tests.Tools.Generation;

public sealed class ToolCatalogContractValidatorTests
{
    [Fact]
    public void ValidateRequiredFields_WhenCatalogIsValid_ReturnsNoErrors()
    {
        var errors = ToolCatalogContractValidator.ValidateRequiredFields(CreateValidTools());
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateRequiredFields_WhenRequiredFieldIsMissing_ReturnsError()
    {
        var tools = CreateValidTools().ToList();
        var missionCreate = tools.Single(tool => tool.Name == "mission_create");
        missionCreate.InputSchema["required"] = new JsonArray("name");

        var errors = ToolCatalogContractValidator.ValidateRequiredFields(tools);

        errors.Should().Contain(error => error.Contains("mission_create", StringComparison.Ordinal));
        errors.Should().Contain(error => error.Contains("startDate", StringComparison.Ordinal));
    }

    private static IReadOnlyList<McpToolDefinition> CreateValidTools()
    {
        return
        [
            CreateTool("mission_create", ["name", "startDate", "endDate", "status"]),
            CreateTool("mission_get", ["id"]),
            CreateTool("mission_list", []),
            CreateTool("mission_update", ["id", "payload"]),
            CreateTool("mission_delete", ["id"]),
            CreateTool("mission_indicator_create", ["missionId", "name", "type"]),
            CreateTool("mission_indicator_get", ["id"]),
            CreateTool("mission_indicator_list", []),
            CreateTool("mission_indicator_update", ["id", "payload"]),
            CreateTool("mission_indicator_delete", ["id"]),
            CreateTool("indicator_checkin_create", ["checkinDate", "confidenceLevel"]),
            CreateTool("indicator_checkin_get", ["indicatorId", "checkinId"]),
            CreateTool("indicator_checkin_list", []),
            CreateTool("indicator_checkin_update", ["indicatorId", "checkinId", "payload"]),
            CreateTool("indicator_checkin_delete", ["indicatorId", "checkinId"])
        ];
    }

    private static McpToolDefinition CreateTool(string name, IEnumerable<string> requiredFields)
    {
        var required = new JsonArray(requiredFields.Select(field => (JsonNode?)field).ToArray());
        return new McpToolDefinition(
            name,
            name,
            new JsonObject
            {
                ["type"] = "object",
                ["required"] = required,
                ["properties"] = new JsonObject()
            });
    }
}
