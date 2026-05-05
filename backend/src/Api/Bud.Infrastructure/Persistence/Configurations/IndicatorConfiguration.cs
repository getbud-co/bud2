using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class IndicatorConfiguration : IEntityTypeConfiguration<Indicator>
{
    public void Configure(EntityTypeBuilder<Indicator> builder)
    {
        builder.Property(i => i.Title)
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.UnitLabel)
            .HasMaxLength(50);

        builder.Property(i => i.PeriodLabel)
            .HasMaxLength(100);

        builder.Property(i => i.ExternalConfig)
            .HasMaxLength(2000);

        builder.Property(i => i.SortOrder)
            .HasMaxLength(255);

        builder.Property(i => i.MeasurementMode)
            .HasConversion<string>();

        builder.Property(i => i.GoalType)
            .HasConversion<string>();

        builder.Property(i => i.Unit)
            .HasConversion<string>();

        builder.Property(i => i.Status)
            .HasConversion<string>();

        builder.Property(i => i.ExternalSource)
            .HasConversion<string>();

        builder.HasOne(i => i.Organization)
            .WithMany()
            .HasForeignKey(i => i.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.OrganizationId);
        builder.HasIndex(i => i.MissionId);

        builder.HasOne(i => i.Employee)
            .WithMany()
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing sub-indicators
        builder.HasOne(i => i.ParentKr)
            .WithMany(i => i.SubIndicators)
            .HasForeignKey(i => i.ParentKrId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.LinkedMission)
            .WithMany()
            .HasForeignKey(i => i.LinkedMissionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Checkins)
            .WithOne(c => c.Indicator)
            .HasForeignKey(c => c.IndicatorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
