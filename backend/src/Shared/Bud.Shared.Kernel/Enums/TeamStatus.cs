using System.Text.Json.Serialization;

namespace Bud.Shared.Kernel.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TeamStatus
{
    Active = 0,
    Archived = 1
}
