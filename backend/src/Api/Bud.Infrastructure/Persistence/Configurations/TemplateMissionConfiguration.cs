using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class TemplateMissionConfiguration : IEntityTypeConfiguration<TemplateMission>
{
    public void Configure(EntityTypeBuilder<TemplateMission> builder)
    {
        builder.Property(tg => tg.Name)
            .HasMaxLength(200);

        builder.Property(tg => tg.Description)
            .HasMaxLength(1000);

        builder.Property(tg => tg.Dimension)
            .HasMaxLength(100);

        builder.HasOne(tg => tg.Organization)
            .WithMany()
            .HasForeignKey(tg => tg.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(tg => tg.OrganizationId);
        builder.HasIndex(tg => tg.TemplateId);

        // Self-referencing relationship for hierarchical template missions
        builder.HasOne(tg => tg.Parent)
            .WithMany(tg => tg.Children)
            .HasForeignKey(tg => tg.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(tg => tg.ParentId);
    }
}
