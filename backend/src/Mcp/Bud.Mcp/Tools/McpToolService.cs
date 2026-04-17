using System.Text.Json;
using System.Text.Json.Nodes;
using Bud.Mcp.Auth;

namespace Bud.Mcp.Tools;

public sealed class McpToolService(BudApiSession session)
{
    private static readonly IReadOnlyList<McpToolDefinition> ToolDefinitions =
    [
        CreateTool(
            "auth_login",
            "Autentica a sessão MCP com um e-mail válido.",
            new JsonObject
            {
                ["type"] = "object",
                ["additionalProperties"] = false,
                ["required"] = new JsonArray("email"),
                ["properties"] = new JsonObject
                {
                    ["email"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["format"] = "email"
                    }
                }
            }),
        CreateTool(
            "auth_whoami",
            "Retorna o contexto autenticado atual da sessão MCP.",
            EmptySchema()),
        CreateTool(
            "tenant_list_available",
            "Lista organizações disponíveis para o usuário autenticado.",
            EmptySchema()),
        CreateTool(
            "tenant_set_current",
            "Define a organização atual da sessão MCP.",
            new JsonObject
            {
                ["type"] = "object",
                ["additionalProperties"] = false,
                ["required"] = new JsonArray("tenantId"),
                ["properties"] = new JsonObject
                {
                    ["tenantId"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["format"] = "uuid"
                    }
                }
            }),
        CreateTool(
            "help_list_actions",
            "Lista as ações MCP disponíveis no host atual.",
            EmptySchema()),
        CreateTool(
            "help_action_schema",
            "Retorna schema, campos obrigatórios e exemplo para uma ação MCP.",
            new JsonObject
            {
                ["type"] = "object",
                ["additionalProperties"] = false,
                ["properties"] = new JsonObject
                {
                    ["action"] = new JsonObject { ["type"] = "string" }
                }
            })
    ];

    private static readonly Dictionary<string, McpToolDefinition> ToolMap =
        ToolDefinitions.ToDictionary(tool => tool.Name, tool => tool, StringComparer.Ordinal);

    private readonly BudApiSession _session = session;

    public IReadOnlyList<McpToolDefinition> GetTools() => ToolDefinitions;

    public async Task<JsonNode> ExecuteAsync(string name, JsonElement arguments, CancellationToken cancellationToken = default)
    {
        ValidateRequiredArguments(name, arguments);

        return name switch
        {
            "auth_login" => await LoginAsync(arguments, cancellationToken),
            "auth_whoami" => WhoAmI(),
            "tenant_list_available" => await TenantListAvailableAsync(cancellationToken),
            "tenant_set_current" => await TenantSetCurrentAsync(arguments, cancellationToken),
            "help_list_actions" => HelpListActions(),
            "help_action_schema" => HelpActionSchema(arguments),
            _ => throw new InvalidOperationException($"Tool '{name}' não é suportada.")
        };
    }

    private async Task<JsonNode> LoginAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var email = GetRequiredString(arguments, "email");
        await _session.LoginAsync(email, cancellationToken);

        return new JsonObject
        {
            ["whoami"] = WhoAmI(),
            ["nextSteps"] = new JsonArray("tenant_list_available", "tenant_set_current", "help_list_actions", "help_action_schema"),
            ["message"] = "Sessão autenticada com sucesso. Selecione um tenant antes de executar operações dependentes de organização."
        };
    }

    private JsonObject WhoAmI()
    {
        var auth = _session.AuthContext ?? throw new InvalidOperationException("Sessão MCP não autenticada.");
        return new JsonObject
        {
            ["email"] = auth.Email,
            ["displayName"] = auth.DisplayName,
            ["isGlobalAdmin"] = auth.IsGlobalAdmin,
            ["employeeId"] = auth.EmployeeId?.ToString(),
            ["organizationId"] = auth.OrganizationId?.ToString(),
            ["currentTenantId"] = _session.CurrentTenantId?.ToString()
        };
    }

    private async Task<JsonObject> TenantListAvailableAsync(CancellationToken cancellationToken)
    {
        var organizations = await _session.ListAvailableTenantsAsync(cancellationToken);
        return new JsonObject
        {
            ["items"] = JsonSerializer.SerializeToNode(organizations),
            ["currentTenantId"] = _session.CurrentTenantId?.ToString()
        };
    }

    private async Task<JsonObject> TenantSetCurrentAsync(JsonElement arguments, CancellationToken cancellationToken)
    {
        var tenantId = ParseGuid(GetRequiredString(arguments, "tenantId"), "tenantId");
        await _session.SetCurrentTenantAsync(tenantId, cancellationToken);

        return new JsonObject
        {
            ["tenantId"] = tenantId.ToString(),
            ["message"] = "Tenant atualizado com sucesso."
        };
    }

    private static JsonObject HelpListActions()
    {
        var actions = new JsonArray();
        foreach (var tool in ToolDefinitions)
        {
            actions.Add(BuildActionHelp(tool));
        }

        return new JsonObject { ["actions"] = actions };
    }

    private static JsonObject HelpActionSchema(JsonElement arguments)
    {
        var action = TryGetString(arguments, "action");
        if (string.IsNullOrWhiteSpace(action))
        {
            return HelpListActions();
        }

        if (!ToolMap.TryGetValue(action, out var tool))
        {
            throw new InvalidOperationException($"Ação '{action}' não está disponível.");
        }

        var required = tool.InputSchema["required"]?.AsArray() ?? [];
        return new JsonObject
        {
            ["name"] = tool.Name,
            ["description"] = tool.Description,
            ["required"] = required.DeepClone(),
            ["inputSchema"] = tool.InputSchema.DeepClone(),
            ["example"] = BuildExample(tool.Name)
        };
    }

    private static JsonObject BuildActionHelp(McpToolDefinition tool)
    {
        return new JsonObject
        {
            ["name"] = tool.Name,
            ["description"] = tool.Description,
            ["required"] = tool.InputSchema["required"]?.DeepClone() ?? new JsonArray(),
            ["inputSchema"] = tool.InputSchema.DeepClone()
        };
    }

    private static JsonObject BuildExample(string toolName)
    {
        return toolName switch
        {
            "auth_login" => new JsonObject { ["email"] = "admin@empresa.com" },
            "tenant_set_current" => new JsonObject { ["tenantId"] = "00000000-0000-0000-0000-000000000001" },
            _ => new JsonObject()
        };
    }

    private static string GetRequiredString(JsonElement arguments, string propertyName)
    {
        var value = TryGetString(arguments, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Parâmetro obrigatório ausente: {propertyName}.");
        }

        return value;
    }

    private static string? TryGetString(JsonElement arguments, string propertyName)
    {
        if (arguments.ValueKind != JsonValueKind.Object ||
            !arguments.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return property.GetString();
    }

    private static void ValidateRequiredArguments(string toolName, JsonElement arguments)
    {
        if (!ToolMap.TryGetValue(toolName, out var tool))
        {
            throw new InvalidOperationException($"Tool '{toolName}' não é suportada.");
        }

        if (arguments.ValueKind == JsonValueKind.Undefined)
        {
            arguments = JsonDocument.Parse("{}").RootElement;
        }

        foreach (var item in tool.InputSchema["required"]?.AsArray() ?? [])
        {
            var propertyName = item?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                continue;
            }

            if (arguments.ValueKind != JsonValueKind.Object || !arguments.TryGetProperty(propertyName, out _))
            {
                throw new InvalidOperationException($"Parâmetro obrigatório ausente: {propertyName}.");
            }
        }
    }

    private static Guid ParseGuid(string rawValue, string propertyName)
    {
        if (!Guid.TryParse(rawValue, out var value))
        {
            throw new InvalidOperationException($"Parâmetro '{propertyName}' deve ser um GUID válido.");
        }

        return value;
    }

    private static McpToolDefinition CreateTool(string name, string description, JsonObject inputSchema)
        => new(name, description, inputSchema);

    private static JsonObject EmptySchema()
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["properties"] = new JsonObject()
        };
    }
}
