using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(100);

        builder.Property(t => t.Color)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(t => new { t.OrganizationId, t.Name })
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        builder.HasOne(t => t.Organization)
            .WithMany()
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.OrganizationId);
    }
}
