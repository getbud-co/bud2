using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.Property(g => g.Name)
            .HasMaxLength(200);

        builder.Property(g => g.Description)
            .HasMaxLength(1000);

        builder.Property(g => g.Dimension)
            .HasMaxLength(100);

        builder.HasOne(g => g.Organization)
            .WithMany()
            .HasForeignKey(g => g.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.OrganizationId);

        // Self-referencing relationship for hierarchical missions
        builder.HasOne(g => g.Parent)
            .WithMany(g => g.Children)
            .HasForeignKey(g => g.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(g => g.ParentId);

        builder.HasOne(g => g.Employee)
            .WithMany()
            .HasForeignKey(g => g.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.EmployeeId);

        builder.HasMany(g => g.Indicators)
            .WithOne(i => i.Mission)
            .HasForeignKey(i => i.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Tasks)
            .WithOne(t => t.Mission)
            .HasForeignKey(t => t.MissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
