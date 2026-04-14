namespace Bud.Domain.Tags;

public sealed class Tag : ITenantEntity, IAggregateRoot
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public TeamColor Color { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<MissionTag> MissionTags { get; set; } = [];

    public static Tag Create(
        Guid id,
        Guid organizationId,
        string name,
        TeamColor color)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Tag deve pertencer a uma organização válida.");
        }

        var tag = new Tag
        {
            Id = id,
            OrganizationId = organizationId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        tag.ApplyDetails(name, color);
        return tag;
    }

    public void UpdateDetails(string name, TeamColor color)
    {
        ApplyDetails(name, color);
        UpdatedAt = DateTime.UtcNow;
    }

    private void ApplyDetails(string name, TeamColor color)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new DomainInvariantException("O nome da tag é obrigatório e deve ter até 100 caracteres.");
        }

        if (!Enum.IsDefined(color))
        {
            throw new DomainInvariantException("Cor da tag inválida.");
        }

        Name = name.Trim();
        Color = color;
    }
}
