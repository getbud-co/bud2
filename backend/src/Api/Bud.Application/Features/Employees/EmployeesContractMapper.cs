namespace Bud.Application.Features.Employees;

public static class EmployeesContractMapper
{
    public static EmployeeResponse ToEmployeeResponse(this Employee source)
    {
        return new EmployeeResponse
        {
            Id = source.Id,
            FullName = source.FullName.Value,
            Email = source.Email.Value,
            Role = source.Role,
            OrganizationId = source.OrganizationId,
            IsGlobalAdmin = source.IsGlobalAdmin
        };
    }
}
