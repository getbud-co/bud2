using System.Text.Json.Nodes;

namespace Bud.Mcp.Tools.Generation;

public static class ToolCatalogContractValidator
{
    private static readonly Dictionary<string, string[]> RequiredByTool = new(StringComparer.Ordinal)
    {
        ["mission_create"] = ["name", "startDate", "endDate", "status"],
        ["mission_indicator_create"] = ["missionId", "name", "type"],
        ["indicator_checkin_create"] = ["checkinDate", "confidenceLevel"],
        ["mission_update"] = ["id", "payload"],
        ["mission_indicator_update"] = ["id", "payload"],
        ["indicator_checkin_update"] = ["indicatorId", "checkinId", "payload"],
        ["mission_get"] = ["id"],
        ["mission_delete"] = ["id"],
        ["mission_indicator_get"] = ["id"],
        ["mission_indicator_delete"] = ["id"],
        ["indicator_checkin_get"] = ["indicatorId", "checkinId"],
        ["indicator_checkin_delete"] = ["indicatorId", "checkinId"]
    };

    public static IReadOnlyList<string> ValidateRequiredFields(IReadOnlyList<McpToolDefinition> tools)
    {
        var errors = new List<string>();
        var byName = tools.ToDictionary(tool => tool.Name, tool => tool, StringComparer.Ordinal);

        foreach (var (toolName, requiredProperties) in RequiredByTool)
        {
            if (!byName.TryGetValue(toolName, out var tool))
            {
                errors.Add($"Tool obrigatória ausente no catálogo: {toolName}.");
                continue;
            }

            var requiredInSchema = GetRequiredSet(tool.InputSchema);
            foreach (var requiredProperty in requiredProperties)
            {
                if (!requiredInSchema.Contains(requiredProperty))
                {
                    errors.Add($"Tool '{toolName}' sem campo obrigatório '{requiredProperty}' no schema.");
                }
            }
        }

        return errors;
    }

    private static HashSet<string> GetRequiredSet(JsonObject schema)
    {
        if (schema["required"] is not JsonArray requiredArray)
        {
            return [];
        }

        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in requiredArray)
        {
            var value = entry?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                set.Add(value);
            }
        }

        return set;
    }
}
