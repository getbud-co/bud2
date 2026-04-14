using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class MissionTagConfiguration : IEntityTypeConfiguration<MissionTag>
{
    public void Configure(EntityTypeBuilder<MissionTag> builder)
    {
        builder.HasKey(mt => new { mt.MissionId, mt.TagId });

        builder.HasOne(mt => mt.Mission)
            .WithMany(m => m.Tags)
            .HasForeignKey(mt => mt.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mt => mt.Tag)
            .WithMany(t => t.MissionTags)
            .HasForeignKey(mt => mt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mt => mt.MissionId);
        builder.HasIndex(mt => mt.TagId);
    }
}
