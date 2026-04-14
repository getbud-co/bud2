using Bud.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace Bud.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    private readonly Guid? _tenantId;
    private readonly bool _isGlobalAdmin;
    private readonly string? _userEmail;
    private readonly bool _applyTenantFilter;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _applyTenantFilter = tenantProvider is not null;
        _tenantId = tenantProvider?.TenantId;
        _isGlobalAdmin = tenantProvider?.IsGlobalAdmin ?? false;
        _userEmail = tenantProvider?.UserEmail;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<OrganizationEmployeeMember> OrganizationEmployeeMembers => Set<OrganizationEmployeeMember>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<Indicator> Indicators => Set<Indicator>();
    public DbSet<EmployeeTeam> EmployeeTeams => Set<EmployeeTeam>();
    public DbSet<Checkin> Checkins => Set<Checkin>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateMission> TemplateMissions => Set<TemplateMission>();
    public DbSet<TemplateIndicator> TemplateIndicators => Set<TemplateIndicator>();
    public DbSet<EmployeeAccessLog> EmployeeAccessLogs => Set<EmployeeAccessLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Cycle> Cycles => Set<Cycle>();
    public DbSet<MissionTask> MissionTasks => Set<MissionTask>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<MissionTag> MissionTags => Set<MissionTag>();

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
        // Tenant isolation for employees is enforced through OrganizationEmployeeMember.

        modelBuilder.Entity<OrganizationEmployeeMember>()
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

        modelBuilder.Entity<Mission>()
            .HasQueryFilter(g =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && g.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<Indicator>()
            .HasQueryFilter(i =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && i.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<Checkin>()
            .HasQueryFilter(c =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && c.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<Template>()
            .HasQueryFilter(mt =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && mt.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<TemplateMission>()
            .HasQueryFilter(tg =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && tg.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<TemplateIndicator>()
            .HasQueryFilter(ti =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && ti.OrganizationId == _tenantId)
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

        modelBuilder.Entity<MissionTask>()
            .HasQueryFilter(gt =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && gt.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<Cycle>()
            .HasQueryFilter(c =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && c.OrganizationId == _tenantId)
            );

        modelBuilder.Entity<Tag>()
            .HasQueryFilter(t =>
                !_applyTenantFilter ||
                (_isGlobalAdmin && _tenantId == null) ||
                (_tenantId != null && t.OrganizationId == _tenantId)
            );
    }
}
