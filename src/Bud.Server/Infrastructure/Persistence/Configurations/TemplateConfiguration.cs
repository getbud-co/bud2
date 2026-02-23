using Bud.Server.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Server.Infrastructure.Persistence.Configurations;

public sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.Property(mt => mt.Name)
            .HasMaxLength(200);

        builder.Property(mt => mt.Description)
            .HasMaxLength(1000);

        builder.Property(mt => mt.MissionNamePattern)
            .HasMaxLength(200);

        builder.Property(mt => mt.MissionDescriptionPattern)
            .HasMaxLength(1000);

        builder.HasOne(mt => mt.Organization)
            .WithMany()
            .HasForeignKey(mt => mt.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(mt => mt.OrganizationId);

        builder.HasMany(mt => mt.Metrics)
            .WithOne(mtm => mtm.Template)
            .HasForeignKey(mtm => mtm.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mt => mt.Objectives)
            .WithOne(mto => mto.Template)
            .HasForeignKey(mto => mto.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
