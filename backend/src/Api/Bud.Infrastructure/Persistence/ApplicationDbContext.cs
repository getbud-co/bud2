using Bud.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    private readonly Guid? _tenantId;
    private readonly bool _isGlobalAdmin;
    private readonly bool _applyTenantFilter;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _applyTenantFilter = tenantProvider is not null;
        _tenantId = tenantProvider?.TenantId;
        _isGlobalAdmin = tenantProvider?.IsGlobalAdmin ?? false;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<EmployeeTeam> EmployeeTeams => Set<EmployeeTeam>();
    public DbSet<EmployeeAccessLog> EmployeeAccessLogs => Set<EmployeeAccessLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Cycle> Cycles => Set<Cycle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global Query Filters for multi-tenancy (simplified for performance)
        // SECURITY: Tenant ownership validation is now done in TenantRequiredMiddleware
        // These filters provide basic data isolation only
        // Global admins see all ONLY when no tenant is selected; otherwise they see the selected tenant
        modelBuilder.Entity<Organization>()
            .HasQueryFilter(o =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && o.Id == _tenantId)
            );

        modelBuilder.Entity<Team>()
            .HasQueryFilter(t =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && t.OrganizationId == _tenantId)
            );

        // Employee is now a global identity entity – no tenant filter applied.
        // Tenant isolation for employees is enforced through Membership.

        modelBuilder.Entity<Membership>()
            .HasQueryFilter(m =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && m.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<EmployeeTeam>()
            .HasQueryFilter(ct =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && ct.Employee.Memberships.Any(m => m.OrganizationId == _tenantId))
            );

        modelBuilder.Entity<EmployeeAccessLog>()
            .HasQueryFilter(cal =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && cal.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<Notification>()
            .HasQueryFilter(n =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && n.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<Cycle>()
            .HasQueryFilter(c =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && c.OrganizationId == _tenantId)
            );
    }
}
