using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class MissionMemberConfiguration : IEntityTypeConfiguration<MissionMember>
{
    public void Configure(EntityTypeBuilder<MissionMember> builder)
    {
        builder.HasKey(mm => new { mm.MissionId, mm.EmployeeId });

        builder.Property(mm => mm.Role)
            .HasConversion<string>();

        builder.HasOne(mm => mm.Mission)
            .WithMany(m => m.Members)
            .HasForeignKey(mm => mm.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mm => mm.Employee)
            .WithMany()
            .HasForeignKey(mm => mm.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mm => mm.AddedBy)
            .WithMany()
            .HasForeignKey(mm => mm.AddedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(mm => mm.MissionId);
        builder.HasIndex(mm => mm.EmployeeId);
    }
}
