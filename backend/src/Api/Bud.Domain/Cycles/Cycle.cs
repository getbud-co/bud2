using System.ComponentModel.DataAnnotations.Schema;

namespace Bud.Domain.Cycles;

public sealed class Cycle : ITenantEntity, IAggregateRoot
{

    public Guid Id { get; set; }

    // Organization
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public CycleCadence Cadence { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CycleStatus Status { get; set; }

    public static Cycle Create(
        Guid id,
        Guid organizationId,
        string name,
        CycleCadence cadence,
        DateTime startDate,
        DateTime endDate,
        CycleStatus status,
        Guid? actorCollaboratorId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("Ciclo deve pertencer a uma organização válida.");
        }

        var cycle = new Cycle
        {
            Id = id,
            OrganizationId = organizationId
        };

        cycle.ApplyDetails(name, cadence, startDate, endDate, status);
        return cycle;
    }

    public void UpdateDetails(
        string name,
        CycleCadence cadence,
        DateTime startDate,
        DateTime endDate,
        CycleStatus status)
    {
        ApplyDetails(name, cadence, startDate, endDate, status);
    }

    private void ApplyDetails(
        string name,
        CycleCadence cadence,
        DateTime startDate,
        DateTime endDate,
        CycleStatus status)
    {
        if (!EntityName.TryCreate(name, out var entityName))
        {
            throw new DomainInvariantException("O nome do ciclo é obrigatório e deve ter até 200 caracteres.");
        }

        if (endDate < startDate)
        {
            throw new DomainInvariantException("Data de término deve ser igual ou posterior à data de início.");
        }

        if (!Enum.IsDefined(cadence))
        {
            throw new DomainInvariantException("Cadência do ciclo inválida.");
        }

        Name = entityName.Value;
        Cadence = cadence;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
    }

}
