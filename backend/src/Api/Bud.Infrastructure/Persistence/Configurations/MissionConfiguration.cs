using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.Property(m => m.Title)
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.Dimension)
            .HasMaxLength(100);

        builder.Property(m => m.SortOrder)
            .HasMaxLength(255);

        builder.Property(m => m.Path)
            .HasColumnType("jsonb");

        builder.Property(m => m.Status)
            .HasConversion<string>();

        builder.Property(m => m.Visibility)
            .HasConversion<string>();

        builder.Property(m => m.KanbanStatus)
            .HasConversion<string>();

        builder.HasOne(m => m.Organization)
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.OrganizationId);

        builder.HasOne(m => m.Cycle)
            .WithMany()
            .HasForeignKey(m => m.CycleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.ParentId);

        builder.HasOne(m => m.Employee)
            .WithMany()
            .HasForeignKey(m => m.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.EmployeeId);

        builder.HasMany(m => m.Members)
            .WithOne(mm => mm.Mission)
            .HasForeignKey(mm => mm.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Indicators)
            .WithOne(i => i.Mission)
            .HasForeignKey(i => i.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Tasks)
            .WithOne(t => t.Mission)
            .HasForeignKey(t => t.MissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
