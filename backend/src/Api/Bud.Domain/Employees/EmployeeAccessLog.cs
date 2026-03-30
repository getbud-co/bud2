namespace Bud.Domain.Employees;

public sealed class EmployeeAccessLog : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public DateTime AccessedAt { get; set; }

    public static EmployeeAccessLog Create(Guid id, Guid employeeId, Guid organizationId, DateTime accessedAt)
    {
        if (employeeId == Guid.Empty)
        {
            throw new DomainInvariantException("O log de acesso deve possuir um colaborador válido.");
        }

        if (organizationId == Guid.Empty)
        {
            throw new DomainInvariantException("O log de acesso deve possuir uma organização válida.");
        }

        return new EmployeeAccessLog
        {
            Id = id,
            EmployeeId = employeeId,
            OrganizationId = organizationId,
            AccessedAt = accessedAt
        };
    }
}
