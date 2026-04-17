using Bud.Application.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bud.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IOptions<GlobalAdminSettings> settings)
    {
        var organizationName = settings.Value.OrganizationName?.Trim();
        var adminEmail = settings.Value.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(organizationName) || string.IsNullOrWhiteSpace(adminEmail))
        {
            return;
        }

        var organization = await context.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Name == organizationName);

        if (organization is null)
        {
            organization = Organization.Create(Guid.NewGuid(), organizationName);
            context.Organizations.Add(organization);
            await context.SaveChangesAsync();
        }

        var admin = await context.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Email == adminEmail);

        if (admin is null)
        {
            admin = Employee.Create(
                Guid.NewGuid(),
                organization.Id,
                "Administrador Global",
                adminEmail,
                EmployeeRole.Leader);
            admin.IsGlobalAdmin = true;

            context.Employees.Add(admin);
            await context.SaveChangesAsync();
            return;
        }

        var changed = false;

        if (admin.OrganizationId != organization.Id)
        {
            admin.OrganizationId = organization.Id;
            changed = true;
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
