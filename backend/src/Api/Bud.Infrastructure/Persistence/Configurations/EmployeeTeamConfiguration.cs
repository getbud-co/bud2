using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class EmployeeTeamConfiguration : IEntityTypeConfiguration<EmployeeTeam>
{
    public void Configure(EntityTypeBuilder<EmployeeTeam> builder)
    {
        builder.HasKey(ct => new { ct.EmployeeId, ct.TeamId });

        builder.Property(ct => ct.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(ct => ct.Employee)
            .WithMany(c => c.EmployeeTeams)
            .HasForeignKey(ct => ct.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ct => ct.Team)
            .WithMany(t => t.EmployeeTeams)
            .HasForeignKey(ct => ct.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ct => ct.EmployeeId);
        builder.HasIndex(ct => ct.TeamId);
    }
}
