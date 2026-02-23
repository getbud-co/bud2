using Bud.Server.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Server.Infrastructure.Persistence.Configurations;

public sealed class TemplateMetricConfiguration : IEntityTypeConfiguration<TemplateMetric>
{
    public void Configure(EntityTypeBuilder<TemplateMetric> builder)
    {
        builder.Property(mtm => mtm.Name)
            .HasMaxLength(200);

        builder.Property(mtm => mtm.TargetText)
            .HasMaxLength(1000);

        builder.HasOne(mtm => mtm.Organization)
            .WithMany()
            .HasForeignKey(mtm => mtm.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mtm => mtm.TemplateObjective)
            .WithMany(mto => mto.Metrics)
            .HasForeignKey(mtm => mtm.TemplateObjectiveId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(mtm => mtm.OrganizationId);
        builder.HasIndex(mtm => mtm.TemplateId);
        builder.HasIndex(mtm => mtm.TemplateObjectiveId);
    }
}
