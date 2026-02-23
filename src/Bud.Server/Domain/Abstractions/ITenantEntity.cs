namespace Bud.Server.Domain.Abstractions;

public interface ITenantEntity
{
    Guid OrganizationId { get; set; }
}
