namespace Bud.Application.Features.Employees;

public static class EmployeesContractMapper
{
    public static EmployeeResponse ToEmployeeResponse(this Employee source)
    {
        return new EmployeeResponse
        {
            Id = source.Id,
            FullName = source.FullName,
            Email = source.Email,
            Role = source.Role,
            OrganizationId = source.OrganizationId,
            IsGlobalAdmin = source.IsGlobalAdmin
        };
    }
}
