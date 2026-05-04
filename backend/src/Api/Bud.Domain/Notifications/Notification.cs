namespace Bud.Domain.Notifications;

public sealed class Notification : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }
    public Guid RecipientEmployeeId { get; set; }
    public Employee RecipientEmployee { get; set; } = null!;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }

    public static Notification Create(
        Guid id,
        Guid recipientEmployeeId,
        Guid organizationId,
        string title,
        string message,
        string category,
        DateTime createdAtUtc,
        Guid? referenceId = null,
        string? referenceType = null)
    {
        if (recipientEmployeeId == Guid.Empty)
        {
            throw new DomainInvariantException("A notificação deve ter um destinatário válido.");
        }

        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("A notificação deve pertencer a uma organização válida.");
        }

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
        {
            throw new DomainInvariantException("O título da notificação é obrigatório e deve ter até 200 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(message) || message.Trim().Length > 1000)
        {
            throw new DomainInvariantException("A mensagem da notificação é obrigatória e deve ter até 1000 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 100)
        {
            throw new DomainInvariantException("A categoria da notificação é obrigatória e deve ter até 100 caracteres.");
        }

        return new Notification
        {
            Id = id,
            RecipientEmployeeId = recipientEmployeeId,
            OrganizationId = organizationId,
            Title = title.Trim(),
            Message = message.Trim(),
            Category = category.Trim(),
            IsRead = false,
            CreatedAtUtc = createdAtUtc,
            ReferenceId = referenceId,
            ReferenceType = string.IsNullOrWhiteSpace(referenceType) ? null : referenceType.Trim()
        };
    }

    public void MarkAsRead(DateTime readAtUtc)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = readAtUtc;
    }
}
