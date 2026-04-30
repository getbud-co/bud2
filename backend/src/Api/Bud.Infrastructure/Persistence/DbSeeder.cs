using Bud.Application.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bud.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IOptions<GlobalAdminSettings> settings)
    {
        if (!OrganizationDomainName.TryCreate(settings.Value.OrganizationName, out var organizationDomainName)
            || !EmailAddress.TryCreate(settings.Value.Email, out var adminEmailAddress))
        {
            return;
        }

        var organizationName = organizationDomainName.Value;
        var adminEmail = adminEmailAddress.Value;

        var organization = await context.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => EF.Property<string>(o, nameof(Organization.Name)) == organizationName);

        if (organization is null)
        {
            budOrg = Organization.Create(
                Guid.NewGuid(),
                DefaultOrganizationName,
                cnpj: "00.000.000/0001-00",
                OrganizationPlan.Free,
                OrganizationContractStatus.ToApproval);
            context.Organizations.Add(budOrg);
            await context.SaveChangesAsync();
        }

        var adminMember = await context.Memberships
            .IgnoreQueryFilters()
            .Include(m => m.Employee)
            .FirstOrDefaultAsync(m =>
                m.OrganizationId == budOrg.Id &&
                m.Employee.Email == DefaultAdminEmail);

        if (adminMember is null)
        {
            var adminId = Guid.NewGuid();
            var adminEmployee = new Employee
            {
                Id = adminId,
                FullName = "Administrador Global",
                Email = DefaultAdminEmail,
            };
            adminMember = new Membership
            {
                EmployeeId = adminId,
                OrganizationId = budOrg.Id,
                Role = EmployeeRole.TeamLeader,
                IsGlobalAdmin = true,
                Employee = adminEmployee,
            };
            context.Employees.Add(adminEmployee);
            context.Memberships.Add(adminMember);
            await context.SaveChangesAsync();
            return;
        }

        if (!adminMember.IsGlobalAdmin)
        {
            adminMember.IsGlobalAdmin = true;
            await context.SaveChangesAsync();
        }

        if (!admin.IsGlobalAdmin)
        {
            admin.IsGlobalAdmin = true;
            changed = true;
        }

        if (admin.Role != EmployeeRole.Leader)
        {
            admin.Role = EmployeeRole.Leader;
            changed = true;
        }

        if (changed)
        {
            await context.SaveChangesAsync();
        }
    }
}
