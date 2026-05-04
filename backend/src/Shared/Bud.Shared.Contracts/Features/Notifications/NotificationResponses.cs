namespace Bud.Shared.Contracts.Features.Notifications;

public sealed class NotificationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}
