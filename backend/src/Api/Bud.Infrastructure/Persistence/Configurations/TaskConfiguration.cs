using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<MissionTask>
{
    public void Configure(EntityTypeBuilder<MissionTask> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("date")
            .IsRequired(false);

        builder.HasOne(t => t.Organization)
            .WithMany()
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.OrganizationId);

        builder.HasOne(t => t.Mission)
            .WithMany(g => g.Tasks)
            .HasForeignKey(t => t.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.MissionId);
        builder.HasIndex(t => new { t.MissionId, t.State });
    }
}
