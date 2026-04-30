namespace Bud.Domain.Employees;

/// <summary>
/// Raiz do agregado de colaborador. Representa a identidade global do colaborador
/// e é o único ponto de acesso ao seu vínculo organizacional (Membership).
/// </summary>
public sealed class Employee : IAggregateRoot
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public EmployeeLanguage Language { get; set; } = EmployeeLanguage.Pt;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Invited;

    public ICollection<EmployeeTeam> EmployeeTeams { get; set; } = new List<EmployeeTeam>();
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    /// <summary>
    /// Retorna o vínculo organizacional do colaborador. Com o filtro de tenant ativo,
    /// a coleção contém no máximo um item correspondente à organização corrente.
    /// </summary>
    public Membership? GetMembership() => Memberships.FirstOrDefault();

    /// <summary>
    /// Verifica se o colaborador possui o papel mínimo exigido na organização especificada.
    /// </summary>
    public bool HasMinimumRoleIn(Guid organizationId, EmployeeRole minimumRole)
        => Memberships.Any(m => m.OrganizationId == organizationId && m.Role >= minimumRole);

    public static Employee Create(Guid id, string fullName, string email)
    {
        var employee = new Employee { Id = id };
        employee.UpdateIdentity(fullName, email);
        return employee;
    }

    public void UpdateIdentity(string fullName, string email)
    {
        if (!PersonName.TryCreate(fullName, out var personName))
        {
            throw new DomainInvariantException("O nome do colaborador é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainInvariantException("O e-mail do colaborador é obrigatório.");
        }

        FullName = personName.Value;
        Email = email.Trim();
    }
}
