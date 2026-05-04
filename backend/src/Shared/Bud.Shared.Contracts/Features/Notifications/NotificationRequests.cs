using Bud.Shared.Kernel;
using System.Text.Json.Serialization;

namespace Bud.Shared.Contracts.Features.Notifications;

public sealed class PatchNotificationRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<bool> IsRead { get; set; }
}
