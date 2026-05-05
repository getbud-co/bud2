using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<MissionTask>
{
    public void Configure(EntityTypeBuilder<MissionTask> builder)
    {
        builder.Property(t => t.Title)
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.SortOrder)
            .HasMaxLength(255);

        builder.Property(t => t.DueDate)
            .HasColumnType("date");

        builder.HasOne(t => t.Organization)
            .WithMany()
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.OrganizationId);

        builder.HasOne(t => t.Employee)
            .WithMany()
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Mission)
            .WithMany(m => m.Tasks)
            .HasForeignKey(t => t.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.MissionId);
    }
}
