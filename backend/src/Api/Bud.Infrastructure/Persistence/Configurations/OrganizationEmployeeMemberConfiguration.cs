using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class OrganizationEmployeeMemberConfiguration : IEntityTypeConfiguration<OrganizationEmployeeMember>
{
    public void Configure(EntityTypeBuilder<OrganizationEmployeeMember> builder)
    {
        builder.HasKey(m => new { m.EmployeeId, m.OrganizationId });

        builder.Property(m => m.IsGlobalAdmin)
            .HasDefaultValue(false);

        builder.HasOne(m => m.Employee)
            .WithMany(e => e.Memberships)
            .HasForeignKey(m => m.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Organization)
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Team)
            .WithMany()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(m => m.Leader)
            .WithMany()
            .HasForeignKey(m => m.LeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(m => m.OrganizationId);
    }
}
