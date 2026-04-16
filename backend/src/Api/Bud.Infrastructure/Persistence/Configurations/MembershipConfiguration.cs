using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bud.Infrastructure.Persistence.Configurations;

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.HasKey(m => new { m.EmployeeId, m.OrganizationId });

        builder.Property(m => m.IsGlobalAdmin)
            .HasDefaultValue(false);

        builder.HasOne(m => m.Employee)
            .WithMany(e => e.Memberships)
            .HasForeignKey(m => m.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Organization)
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // LeaderId is a self-referencing FK to Employee via Membership, stored as a scalar.
        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(m => m.LeaderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(m => m.OrganizationId);
    }
}
